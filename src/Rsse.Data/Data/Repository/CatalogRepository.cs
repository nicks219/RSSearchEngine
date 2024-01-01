using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SearchEngine.Data.Context;
using SearchEngine.Data.Dto;
using SearchEngine.Data.Entities;
using SearchEngine.Data.Repository.Contracts;
using SearchEngine.Data.Repository.Exceptions;

namespace SearchEngine.Data.Repository;

/// <summary>
/// Репозиторий доступа к бд
/// </summary>
public class CatalogRepository : IDataRepository
{
    private readonly CatalogContext _context;

    /// <summary>
    /// Создать репозиторий
    /// </summary>
    /// <param name="serviceProvider"></param>
    public CatalogRepository(IServiceProvider serviceProvider)
    {
        _context = serviceProvider.GetRequiredService<CatalogContext>();
    }

    /// <inheritdoc/>
    public async Task CreateTagIfNotExists(string tag)
    {
        tag = tag.ToUpper();

        var exists = await _context.Tags!.AnyAsync(tagEntity => tagEntity.Tag == tag);

        if (exists)
        {
            return;
        }

        var maxId = await _context.Tags!.Select(tagEntity => tagEntity.TagId).MaxAsync();

        var newTag = new TagEntity { Tag = tag, TagId = ++maxId };

        await _context.Tags!.AddAsync(newTag);

        await _context.SaveChangesAsync();
    }

    /// <inheritdoc/>
    public IQueryable<NoteEntity> ReadAllNotes()
    {
        var notes =
            _context.Notes!
            .Select(note => note)
            .AsNoTracking();

        return notes;
    }

    /// <inheritdoc/>
    public string ReadNoteTitle(int noteId)
    {
        var note =
            _context.Notes!
            .First(noteEntity => noteEntity.NoteId == noteId);

        return note.Title!;
    }

    /// <inheritdoc/>
    public int ReadNoteId(string noteTitle)
    {
        var note =
            _context.Notes!
            .First(noteEntity => noteEntity.Title == noteTitle);

        return note.NoteId;
    }

    /// <inheritdoc/>
    public IQueryable<int> ReadTaggedNotes(IEnumerable<int> checkedTags)
    {
        // TODO: определить какой метод лучше
        // IQueryable<int> songsCollection = database.GenreText//
        //    .Where(s => chosenOnes.Contains(s.GenreInGenreText.GenreID))
        //    .Select(s => s.TextInGenreText.TextID);

        var notesForElection = _context.Notes!
            .Where(note => note.RelationEntityReference!.Any(relation => checkedTags.Contains(relation.TagId)))
            .Select(note => note.NoteId);

        return notesForElection;
    }

    /// <inheritdoc/>
    public IQueryable<Tuple<string, int>> ReadCatalogPage(int pageNumber, int pageSize)
    {
        var titleAndIdList = _context.Notes!
            .OrderBy(note => note.Title)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(note => new Tuple<string, int>(note.Title!, note.NoteId))
            .AsNoTracking();

        return titleAndIdList;
    }

    /// <inheritdoc/>
    public IQueryable<Tuple<string, string>> ReadNote(int noteId)
    {
        var titleAndText = _context.Notes!
            .Where(note => note.NoteId == noteId)
            .Select(note => new Tuple<string, string>(note.Text!, note.Title!))
            .AsNoTracking();

        return titleAndText;
    }

    /// <inheritdoc/>
    public IQueryable<int> ReadNoteTags(int noteId)
    {
        var songGenres = _context.TagsToNotesRelation!
            .Where(relation => relation.NoteInRelationEntity!.NoteId == noteId)
            .Select(relation => relation.TagId);

        return songGenres;
    }

    /// <inheritdoc/>
    public async Task<UserEntity?> GetUser(LoginDto login)
    {
        return await _context.Users!
            .FirstOrDefaultAsync(user => user.Email == login.Email && user.Password == login.Password);
    }

    /// <inheritdoc/>
    public async Task<int> ReadNotesCount()
    {
        return await _context.Notes!.CountAsync();
    }

    /// <inheritdoc/>
    public async Task<List<string>> ReadStructuredTagList()
    {
        var tagList = await _context.Tags!
            // TODO заменить сортировку на корректный индекс в бд
            .OrderBy(tag => tag.TagId)
            .Select(tag => new Tuple<string, int>(tag.Tag!, tag.RelationEntityReference!.Count))
            .ToListAsync();

        return tagList.Select(tagAndAmount =>
                tagAndAmount.Item2 > 0
                    ? tagAndAmount.Item1 + ": " + tagAndAmount.Item2
                    : tagAndAmount.Item1)
            .ToList();
    }

    /// <inheritdoc/>
    public async Task UpdateNote(IEnumerable<int> initialTags, NoteDto note)
    {
        var forAddition = note.TagsCheckedRequest!.ToHashSet();

        var forDelete = initialTags.ToHashSet();

        var except = forAddition.Intersect(forDelete).ToList();

        forAddition.ExceptWith(except);

        forDelete.ExceptWith(except);

        if (await VerifyTagNotExists(note.CommonNoteId, forAddition))
        {
            // ID тегов и номера кнопок с фронта совпадают
            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var processedNote = await _context.Notes!.FindAsync(note.CommonNoteId);

                if (processedNote == null)
                {
                    throw new Exception($"[{nameof(UpdateNote)}: Null in Text]");
                }

                processedNote.Title = note.TitleRequest;

                processedNote.Text = note.TextRequest;

                _context.Notes.Update(processedNote);

                _context.TagsToNotesRelation!
                    .RemoveRange(_context.TagsToNotesRelation
                        .Where(relation =>
                            relation.NoteId == note.CommonNoteId && forDelete.Contains(relation.TagId)));

                await _context.TagsToNotesRelation
                    .AddRangeAsync(forAddition
                        .Select(id =>
                            new TagsToNotesEntity
                            {
                                NoteId = note.CommonNoteId,
                                TagId = id
                            }));

                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
            }
            catch (DataExistsException)
            {
                await transaction.RollbackAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                throw new Exception($"[{nameof(UpdateNote)}: Repo]", ex);
            }
        }
    }

    /// <inheritdoc/>
    public async Task<int> CreateNote(NoteDto note)
    {
        if (await VerifyTitleNotExists(note.TitleRequest!) == false)
        {
            return note.CommonNoteId;
        }

        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var forAddition = new NoteEntity { Title = note.TitleRequest, Text = note.TextRequest };

            await _context.Notes!.AddAsync(forAddition);

            await _context.SaveChangesAsync();

            await _context.TagsToNotesRelation!
                .AddRangeAsync(note.TagsCheckedRequest!
                    .Select(id =>
                        new TagsToNotesEntity
                        {
                            NoteId = forAddition.NoteId,
                            TagId = id
                        }));

            await _context.SaveChangesAsync();

            await transaction.CommitAsync();

            note.CommonNoteId = forAddition.NoteId;
        }
        catch (DataExistsException)
        {
            await transaction.RollbackAsync();

            note.CommonNoteId = 0;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();

            note.CommonNoteId = 0;

            throw new Exception($"[{nameof(CreateNote)}: Repo]", ex);
        }

        return note.CommonNoteId;
    }

    /// <inheritdoc/>
    public async Task<int> DeleteNote(int noteId)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var deletedEntries = 0;

            var processedNote = await _context.Notes!.FindAsync(noteId);

            if (processedNote == null)
            {
                return deletedEntries;
            }

            _context.Notes.Remove(processedNote);

            deletedEntries = await _context.SaveChangesAsync();

            await transaction.CommitAsync();

            return deletedEntries;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();

            throw new Exception($"[{nameof(DeleteNote)}: Repo]", ex);
        }
    }

    private async Task<bool> VerifyTitleNotExists(string title)
    {
        return !await _context.Notes!.AnyAsync(entity => entity.Title == title);
    }

    private async Task<bool> VerifyTagNotExists(int noteId, IReadOnlyCollection<int> forAddition)
    {
        if (forAddition.Count == 0)
        {
            return true;
        }

        if (await _context.TagsToNotesRelation!.AnyAsync(relation =>
                relation.NoteId == noteId && relation.TagId == forAddition.First()))
        {
            throw new DataExistsException("[PANIC] tags exists error");
        }

        return true;
    }

    public async ValueTask DisposeAsync()
    {
        await _context.DisposeAsync().ConfigureAwait(false);

        GC.SuppressFinalize(this);
    }

    // WARN на старте отрабатывает четыре раза
    void IDisposable.Dispose()
    {
        _context.Dispose();

        GC.SuppressFinalize(this);
    }
}
