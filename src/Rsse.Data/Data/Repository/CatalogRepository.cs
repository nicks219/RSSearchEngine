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

public class CatalogRepository : IDataRepository
{
    private readonly CatalogContext _context;

    public CatalogRepository(IServiceProvider serviceProvider)
    {
        _context = serviceProvider.GetRequiredService<CatalogContext>();
    }

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

    public IQueryable<NoteEntity> ReadAllNotes()
    {
        var notes =
            _context.Notes!
            //.Select(s => string.Concat(s.TextId, " '", s.Title!, "' '", s.Song!,"'"))
            .Select(note => note)
            .AsNoTracking();

        return notes;
    }

    public string ReadNoteTitle(int noteId)
    {
        // см. предупреждение Rider "21 DB commands":
        var note =
            _context.Notes!
            .First(noteEntity => noteEntity.NoteId == noteId);

        return note.Title!;
    }

    public int ReadNoteId(string noteTitle)
    {
        var note =
            _context.Notes!
            .First(noteEntity => noteEntity.Title == noteTitle);

        return note.NoteId;
    }

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

    public IQueryable<Tuple<string, string>> ReadNote(int noteId)
    {
        var titleAndText = _context.Notes!
            .Where(note => note.NoteId == noteId)
            .Select(note => new Tuple<string, string>(note.Text!, note.Title!))
            .AsNoTracking();

        return titleAndText;
    }

    public IQueryable<int> ReadNoteTags(int noteId)
    {
        var songGenres = _context.TagsToNotesRelation!
            .Where(relation => relation.NoteInRelationEntity!.NoteId == noteId)
            .Select(relation => relation.TagId);

        return songGenres;
    }

    public async Task<UserEntity?> GetUser(LoginDto login)
    {
        return await _context.Users!
            .FirstOrDefaultAsync(user => user.Email == login.Email && user.Password == login.Password);
    }

    public async Task<int> ReadNotesCount()
    {
        return await _context.Notes!.CountAsync();
    }

    public async Task<List<string>> ReadGeneralTagList()
    {
        var tagList = await _context.Tags!
            .OrderBy(tag => tag.TagId) // вместо сортировки построй корректный индекс на стороне бд
            .Select(tag => new Tuple<string, int>(tag.Tag!, tag.RelationEntityReference!.Count))
            .ToListAsync();

        return tagList.Select(tagAndAmount =>
                tagAndAmount.Item2 > 0
                    ? tagAndAmount.Item1 + ": " + tagAndAmount.Item2
                    : tagAndAmount.Item1)
            .ToList();
    }

    public async Task UpdateNote(IEnumerable<int> initialTags, NoteDto note)
    {
        var forAddition = note.TagsCheckedRequest!.ToHashSet();

        var forDelete = initialTags.ToHashSet();

        var except = forAddition.Intersect(forDelete).ToList();

        forAddition.ExceptWith(except);

        forDelete.ExceptWith(except);

        if (await VerifyTagNotExists(note.NoteId, forAddition))
        {
            // название заметки остаётся неизменным (constraint)
            // ID тегов и номера кнопок с фронта совпадают
            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // дешевле просто откатить или не начинать транзакцию, без механизма исключений
                var processedNote = await _context.Notes!.FindAsync(note.NoteId);

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
                            relation.NoteId == note.NoteId && forDelete.Contains(relation.TagId)));

                await _context.TagsToNotesRelation
                    .AddRangeAsync(forAddition
                        .Select(id =>
                            new TagsToNotesEntity
                            {
                                NoteId = note.NoteId,
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

    public async Task<int> CreateNote(NoteDto note)
    {
        if (await VerifyTitleNotExists(note.TitleRequest!) == false)
        {
            return note.NoteId;
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

            note.NoteId = forAddition.NoteId;
        }
        catch (DataExistsException)
        {
            await transaction.RollbackAsync();

            note.NoteId = 0;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();

            note.NoteId = 0;

            throw new Exception($"[{nameof(CreateNote)}: Repo]", ex);
        }

        return note.NoteId;
    }

    public async Task<int> DeleteNote(int noteId)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var writtenEntries = 0;

            var processedNote = await _context.Notes!.FindAsync(noteId);

            if (processedNote == null)
            {
                return writtenEntries;
            }

            _context.Notes.Remove(processedNote);

            writtenEntries = await _context.SaveChangesAsync();

            await transaction.CommitAsync();

            return writtenEntries;
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
            throw new DataExistsException("[Global Error: Tags Exists Error]");
        }

        return true;
    }

    public async ValueTask DisposeAsync()
    {
        await _context.DisposeAsync().ConfigureAwait(false);

        // context = null;

        GC.SuppressFinalize(this);
    }

    // на старте отрабатывает четыре раза
    void IDisposable.Dispose()
    {
        _context.Dispose();

        // context = null;

        GC.SuppressFinalize(this);
    }
}
