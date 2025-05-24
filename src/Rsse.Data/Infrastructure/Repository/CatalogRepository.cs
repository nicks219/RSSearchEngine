using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SearchEngine.Data.Contracts;
using SearchEngine.Data.Dto;
using SearchEngine.Data.Entities;
using SearchEngine.Exceptions;
using SearchEngine.Infrastructure.Context;

namespace SearchEngine.Infrastructure.Repository;

/// <summary>
/// Репозиторий слоя данных.
/// </summary>
public sealed class CatalogRepository<T>(T context) : IDataRepository where T : BaseCatalogContext
{
    /// <summary/> Контейнер для метода, создающего обогащенный список тегов.
    private record struct EnrichedTagList(string Tag, int RelationEntityReferenceCount);

    private readonly CancellationToken _rollbackToken = CancellationToken.None;

    /// <inheritdoc/>
    public async Task CreateTagIfNotExists(string tag, CancellationToken ct)
    {
        tag = tag.ToUpper();

        var exists = await context.Tags
            .AnyAsync(tagEntity => tagEntity.Tag == tag, ct);

        if (exists)
        {
            return;
        }

        var maxId = await context.Tags
            .Select(tagEntity => tagEntity.TagId)
            .DefaultIfEmpty()
            .MaxAsync(ct);

        var newTag = new TagEntity { Tag = tag, TagId = ++maxId };

        await context.Tags.AddAsync(newTag, ct);

        await context.SaveChangesAsync(ct);
    }

    /// <inheritdoc/>
    public ConfiguredCancelableAsyncEnumerable<NoteEntity> ReadAllNotes(CancellationToken ct)
    {
        var notes = context.Notes
            .AsNoTracking()
            .AsAsyncEnumerable()
            .WithCancellation(ct);

        return notes;
    }

    /// <inheritdoc/>
    public async Task<string?> ReadNoteTitle(int noteId, CancellationToken ct)
    {
        var title = await context.Notes
            .Where(noteEntity => noteEntity.NoteId == noteId)
            .Select(noteEntity => noteEntity.Title)
            .FirstOrDefaultAsync(ct);

        return title;
    }

    /// <inheritdoc/>
    public async Task<List<int>> ReadTaggedNotesIds(IEnumerable<int> checkedTags, CancellationToken ct)
    {
        // TODO: определить какой метод лучше
        // IQueryable<int> songsCollection = database.GenreText//
        //    .Where(s => chosenOnes.Contains(s.GenreInGenreText.GenreID))
        //    .Select(s => s.TextInGenreText.TextID);

        var noteIds = await context.Notes
            .Where(note => note.RelationEntityReference!.Any(relation => checkedTags.Contains(relation.TagId)))
            .Select(note => note.NoteId)
            .ToListAsync(ct);

        return noteIds;
    }

    /// <inheritdoc/>
    public async Task<List<CatalogItemDto>> ReadCatalogPage(int pageNumber, int pageSize, CancellationToken ct)
    {
        var catalogPages = await context.Notes
            .OrderBy(note => note.Title)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(note => new CatalogItemDto { Title = note.Title!, NoteId = note.NoteId })
            .ToListAsync(ct);

        return catalogPages;
    }

    /// <inheritdoc/>
    public async Task<TextResultDto?> ReadNote(int noteId, CancellationToken ct)
    {
        var note = await context.Notes
            .Where(note => note.NoteId == noteId)
            .Select(note => new TextResultDto { Text = note.Text!, Title = note.Title! })
            .FirstOrDefaultAsync(ct);

        return note;
    }

    /// <inheritdoc/>
    public async Task<List<int>> ReadNoteTagIds(int noteId, CancellationToken ct)
    {
        var tagIds = await context.TagsToNotesRelation
            .Where(relation => relation.NoteInRelationEntity!.NoteId == noteId)
            .Select(relation => relation.TagId)
            .ToListAsync(ct);

        return tagIds;
    }

    /// <inheritdoc/>
    public async Task<UserEntity?> GetUser(CredentialsRequestDto credentialsRequest, CancellationToken ct)
    {
        return await context.Users.FirstOrDefaultAsync(user =>
            user.Email == credentialsRequest.Email && user.Password == credentialsRequest.Password, ct);
    }

    /// <inheritdoc/>
    public async Task<int> ReadNotesCount(CancellationToken ct)
    {
        return await context.Notes.CountAsync(ct);
    }

    /// <inheritdoc/>
    public async Task<List<string>> ReadEnrichedTagList(CancellationToken ct)
    {
        var tagList = await context.Tags
            // todo: [?] заменить сортировку на корректный индекс в бд
            .OrderBy(tag => tag.TagId)
            .Select(tag => new EnrichedTagList(tag.Tag!, tag.RelationEntityReference!.Count))
            .ToListAsync(ct);

        return tagList.Select(tagAndAmount =>
                tagAndAmount.RelationEntityReferenceCount > 0
                    ? tagAndAmount.Tag + ": " + tagAndAmount.RelationEntityReferenceCount
                    : tagAndAmount.Tag)
            .ToList();
    }

    /// <inheritdoc/>
    public async Task UpdateNote(IEnumerable<int> initialTags, NoteRequestDto noteRequest, CancellationToken ct)
    {
        var forAddition = noteRequest.CheckedTags!.ToHashSet();

        var forDelete = initialTags.ToHashSet();

        var except = forAddition.Intersect(forDelete).ToList();

        forAddition.ExceptWith(except);

        forDelete.ExceptWith(except);

        if (await VerifyTagNotExists(noteRequest.NoteIdExchange, forAddition, ct))
        {
            // ID тегов и номера кнопок с фронта совпадают
            await using var transaction = await context.Database.BeginTransactionAsync(ct);

            try
            {
                var processedNote = await context.Notes.FindAsync([noteRequest.NoteIdExchange], cancellationToken: ct);

                if (processedNote == null)
                {
                    throw new Exception($"[{nameof(UpdateNote)}: Null in Text]");
                }

                processedNote.Title = noteRequest.Title;

                processedNote.Text = noteRequest.Text;

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
                            }), ct);

                await context.SaveChangesAsync(ct);

                await transaction.CommitAsync(ct);
            }
            catch (Exception ex) when (ex is DataExistsException or OperationCanceledException)
            {
                await transaction.RollbackAsync(_rollbackToken);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(_rollbackToken);

                throw new Exception($"[{nameof(UpdateNote)}: Repo]", ex);
            }
        }
    }

    /// <inheritdoc/>
    public async Task UpdateCredos(UpdateCredosRequestDto credosRequest, CancellationToken ct)
    {
        await using var transaction = await context.Database.BeginTransactionAsync(ct);

        try
        {
            var processed = await context.Users.FirstOrDefaultAsync(userEntity =>
                userEntity.Email == credosRequest.OldCredos.Email &&
                userEntity.Password == credosRequest.OldCredos.Password, ct);
            if (processed == null)
                throw new InvalidDataException(
                    $"credos '{credosRequest.OldCredos.Email}:{credosRequest.OldCredos.Password}' are invalid");
            processed.Email = credosRequest.NewCredos.Email;
            processed.Password = credosRequest.NewCredos.Password;
            context.Users.Update(processed);
            await context.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(_rollbackToken);
            throw new Exception($"{nameof(UpdateCredos)} | {ex.Message}");
        }
    }

    /// <inheritdoc/>
    public async Task<int> CreateNote(NoteRequestDto noteRequest, CancellationToken ct)
    {
        if (await VerifyTitleNotExists(noteRequest.Title!, ct) == false)
        {
            return noteRequest.NoteIdExchange;
        }

        var createdId = 0;
        await using var transaction = await context.Database.BeginTransactionAsync(ct);

        try
        {
            var forAddition = new NoteEntity { Title = noteRequest.Title, Text = noteRequest.Text };

            await context.Notes.AddAsync(forAddition, ct);

            await context.SaveChangesAsync(ct);

            await context.TagsToNotesRelation
                .AddRangeAsync(noteRequest.CheckedTags!
                    .Select(id =>
                        new TagsToNotesEntity
                        {
                            NoteId = forAddition.NoteId,
                            TagId = id
                        }), ct);

            await context.SaveChangesAsync(ct);

            await transaction.CommitAsync(ct);

            createdId = forAddition.NoteId;
        }
        catch (Exception ex) when (ex is DataExistsException or OperationCanceledException)
        {
            await transaction.RollbackAsync(_rollbackToken);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(_rollbackToken);

            throw new Exception($"[{nameof(CreateNote)}: Repo]", ex);
        }

        return createdId;
    }

    /// <inheritdoc/>
    public async Task<int> DeleteNote(int noteId, CancellationToken ct)
    {
        await using var transaction = await context.Database.BeginTransactionAsync(ct);

        try
        {
            var deletedEntries = 0;

            var processedNote = await context.Notes.FindAsync([noteId], ct);

            if (processedNote == null)
            {
                return deletedEntries;
            }

            context.Notes.Remove(processedNote);

            deletedEntries = await context.SaveChangesAsync(ct);

            await transaction.CommitAsync(ct);

            return deletedEntries;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(_rollbackToken);

            throw new Exception($"[{nameof(DeleteNote)}: Repo]", ex);
        }
    }

    private async Task<bool> VerifyTitleNotExists(string title, CancellationToken ct)
    {
        return !await context.Notes.AnyAsync(entity => entity.Title == title, ct);
    }

    private async Task<bool> VerifyTagNotExists(int noteId, HashSet<int> forAddition, CancellationToken ct)
    {
        if (forAddition.Count == 0)
        {
            return true;
        }

        if (await context.TagsToNotesRelation.AnyAsync(relation =>
                relation.NoteId == noteId && relation.TagId == forAddition.First(), ct))
        {
            throw new DataExistsException("[PANIC] tags exists error");
        }

        return true;
    }
}
