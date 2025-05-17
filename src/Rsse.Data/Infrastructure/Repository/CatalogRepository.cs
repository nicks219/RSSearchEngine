using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SearchEngine.Domain.Contracts;
using SearchEngine.Domain.Dto;
using SearchEngine.Domain.Entities;
using SearchEngine.Infrastructure.Context;
using SearchEngine.Infrastructure.Repository.Exceptions;

namespace SearchEngine.Infrastructure.Repository;

/// <summary>
/// Репозиторий слоя данных
/// </summary>
public class CatalogRepository<T>(T context) : IDataRepository where T : BaseCatalogContext
{
    /// <summary/> Контейнер для метода
    private record struct StructuredTagList(string Tag, int RelationEntityReferenceCount);

    // todo: MySQL WORK. DELETE
    public BaseCatalogContext GetReaderContext() => context;
    public BaseCatalogContext GetPrimaryWriterContext() => context;

    /// <inheritdoc/>
    // todo: MySQL WORK. DELETE
    public Task CopyDbFromMysqlToNpgsql() => throw new NotImplementedException();

    /// <inheritdoc/>
    public async Task CreateTagIfNotExists(string tag)
    {
        tag = tag.ToUpper();

        var exists = await context.Tags.AnyAsync(tagEntity => tagEntity.Tag == tag);

        if (exists)
        {
            return;
        }

        var maxId = await context.Tags
            .Select(tagEntity => tagEntity.TagId)
            .DefaultIfEmpty()
            .MaxAsync();

        var newTag = new TagEntity { Tag = tag, TagId = ++maxId };

        await context.Tags.AddAsync(newTag);

        await context.SaveChangesAsync();
    }

    /// <inheritdoc/>
    public IAsyncEnumerable<NoteEntity> ReadAllNotes()
    {
        var notes = context.Notes
            .AsNoTracking()
            .AsAsyncEnumerable();

        return notes;
    }

    /// <inheritdoc/>
    public async Task<string?> ReadNoteTitle(int noteId)
    {
        var title = await context.Notes
            .Where(noteEntity => noteEntity.NoteId == noteId)
            .Select(noteEntity => noteEntity.Title)
            .FirstOrDefaultAsync();

        return title;
    }

    /// <inheritdoc/>
    public async Task<List<int>> ReadTaggedNotesIds(IEnumerable<int> checkedTags)
    {
        // TODO: определить какой метод лучше
        // IQueryable<int> songsCollection = database.GenreText//
        //    .Where(s => chosenOnes.Contains(s.GenreInGenreText.GenreID))
        //    .Select(s => s.TextInGenreText.TextID);

        var noteIds = await context.Notes
            .Where(note => note.RelationEntityReference!.Any(relation => checkedTags.Contains(relation.TagId)))
            .Select(note => note.NoteId)
            .ToListAsync();

        return noteIds;
    }

    /// <inheritdoc/>
    public async Task<List<CatalogItemDto>> ReadCatalogPage(int pageNumber, int pageSize)
    {
        var catalogPages = await context.Notes
            .OrderBy(note => note.Title)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(note => new CatalogItemDto { Title = note.Title!, NoteId = note.NoteId })
            .ToListAsync();

        return catalogPages;
    }

    /// <inheritdoc/>
    public async Task<TextResult?> ReadNote(int noteId)
    {
        var note = await context.Notes
            .Where(note => note.NoteId == noteId)
            .Select(note => new TextResult { Text = note.Text!, Title = note.Title! })
            .FirstOrDefaultAsync();

        return note;
    }

    /// <inheritdoc/>
    public async Task<List<int>> ReadNoteTagIds(int noteId)
    {
        var tagIds = await context.TagsToNotesRelation
            .Where(relation => relation.NoteInRelationEntity!.NoteId == noteId)
            .Select(relation => relation.TagId)
            .ToListAsync();

        return tagIds;
    }

    /// <inheritdoc/>
    public async Task<UserEntity?> GetUser(CredentialsRequestDto credentialsRequest)
    {
        return await context.Users.FirstOrDefaultAsync(user =>
            user.Email == credentialsRequest.Email && user.Password == credentialsRequest.Password);
    }

    /// <inheritdoc/>
    public async Task<int> ReadNotesCount()
    {
        return await context.Notes.CountAsync();
    }

    /// <inheritdoc/>
    public async Task<List<string>> ReadStructuredTagList()
    {
        var tagList = await context.Tags
            // todo: [?] заменить сортировку на корректный индекс в бд
            .OrderBy(tag => tag.TagId)
            .Select(tag => new StructuredTagList(tag.Tag!, tag.RelationEntityReference!.Count))
            .ToListAsync();

        return tagList.Select(tagAndAmount =>
                tagAndAmount.RelationEntityReferenceCount > 0
                    ? tagAndAmount.Tag + ": " + tagAndAmount.RelationEntityReferenceCount
                    : tagAndAmount.Tag)
            .ToList();
    }

    /// <inheritdoc/>
    public async Task UpdateNote(IEnumerable<int> initialTags, NoteRequestDto noteRequest)
    {
        var forAddition = noteRequest.TagsCheckedRequest!.ToHashSet();

        var forDelete = initialTags.ToHashSet();

        var except = forAddition.Intersect(forDelete).ToList();

        forAddition.ExceptWith(except);

        forDelete.ExceptWith(except);

        if (await VerifyTagNotExists(noteRequest.NoteIdExchange, forAddition))
        {
            // ID тегов и номера кнопок с фронта совпадают
            await using var transaction = await context.Database.BeginTransactionAsync();

            try
            {
                var processedNote = await context.Notes.FindAsync(noteRequest.NoteIdExchange);

                if (processedNote == null)
                {
                    throw new Exception($"[{nameof(UpdateNote)}: Null in Text]");
                }

                processedNote.Title = noteRequest.TitleRequest;

                processedNote.Text = noteRequest.TextRequest;

                context.Notes.Update(processedNote);

                context.TagsToNotesRelation
                    .RemoveRange(context.TagsToNotesRelation
                        .Where(relation =>
                            relation.NoteId == noteRequest.NoteIdExchange && forDelete.Contains(relation.TagId)));

                await context.TagsToNotesRelation
                    .AddRangeAsync(forAddition
                        .Select(id =>
                            new TagsToNotesEntity
                            {
                                NoteId = noteRequest.NoteIdExchange,
                                TagId = id
                            }));

                await context.SaveChangesAsync();

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

    public async Task UpdateCredos(UpdateCredosRequestDto credosRequest)
    {
        await using var transaction = await context.Database.BeginTransactionAsync();

        try
        {
            var processed = await context.Users.FirstOrDefaultAsync(userEntity =>
                userEntity.Email == credosRequest.OldCredos.Email && userEntity.Password == credosRequest.NewCredos.Password);
            if (processed == null) throw new InvalidDataException($"credos '{credosRequest.OldCredos.Email}:{credosRequest.OldCredos.Password}' are invalid");
            processed.Email = credosRequest.NewCredos.Email;
            processed.Password = credosRequest.NewCredos.Password;
            context.Users.Update(processed);
            await context.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            throw new Exception($"{nameof(UpdateCredos)} | {ex.Message}");
        }
    }

    /// <inheritdoc/>
    public async Task<int> CreateNote(NoteRequestDto noteRequest)
    {
        if (await VerifyTitleNotExists(noteRequest.TitleRequest!) == false)
        {
            return noteRequest.NoteIdExchange;
        }

        var createdId = 0;
        await using var transaction = await context.Database.BeginTransactionAsync();

        try
        {
            var forAddition = new NoteEntity { Title = noteRequest.TitleRequest, Text = noteRequest.TextRequest };

            await context.Notes.AddAsync(forAddition);

            await context.SaveChangesAsync();

            await context.TagsToNotesRelation
                .AddRangeAsync(noteRequest.TagsCheckedRequest!
                    .Select(id =>
                        new TagsToNotesEntity
                        {
                            NoteId = forAddition.NoteId,
                            TagId = id
                        }));

            await context.SaveChangesAsync();

            await transaction.CommitAsync();

            createdId = forAddition.NoteId;
        }
        catch (DataExistsException)
        {
            await transaction.RollbackAsync();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();

            throw new Exception($"[{nameof(CreateNote)}: Repo]", ex);
        }

        return createdId;
    }

    /// <inheritdoc/>
    public async Task<int> DeleteNote(int noteId)
    {
        await using var transaction = await context.Database.BeginTransactionAsync();

        try
        {
            var deletedEntries = 0;

            var processedNote = await context.Notes.FindAsync(noteId);

            if (processedNote == null)
            {
                return deletedEntries;
            }

            context.Notes.Remove(processedNote);

            deletedEntries = await context.SaveChangesAsync();

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
        return !await context.Notes.AnyAsync(entity => entity.Title == title);
    }

    private async Task<bool> VerifyTagNotExists(int noteId, IReadOnlyCollection<int> forAddition)
    {
        if (forAddition.Count == 0)
        {
            return true;
        }

        if (await context.TagsToNotesRelation.AnyAsync(relation =>
                relation.NoteId == noteId && relation.TagId == forAddition.First()))
        {
            throw new DataExistsException("[PANIC] tags exists error");
        }

        return true;
    }

    public async ValueTask DisposeAsync()
    {
        await context.DisposeAsync().ConfigureAwait(false);

        GC.SuppressFinalize(this);
    }

    // WARN на старте отрабатывает четыре раза
    void IDisposable.Dispose()
    {
        context.Dispose();

        GC.SuppressFinalize(this);
    }
}
