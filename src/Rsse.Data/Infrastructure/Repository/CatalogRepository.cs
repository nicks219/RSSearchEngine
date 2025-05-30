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

    private readonly CancellationToken _noneToken = CancellationToken.None;

    /// <inheritdoc/>
    public async Task CreateTagIfNotExists(string tag, CancellationToken stoppingToken)
    {
        tag = tag.ToUpper();

        var exists = await context.Tags
            .AnyAsync(tagEntity => tagEntity.Tag == tag, stoppingToken);

        if (exists)
        {
            return;
        }

        var maxId = await context.Tags
            .Select(tagEntity => tagEntity.TagId)
            .DefaultIfEmpty()
            .MaxAsync(stoppingToken);

        var newTag = new TagEntity { Tag = tag, TagId = ++maxId };

        await context.Tags.AddAsync(newTag, stoppingToken);

        await context.SaveChangesAsync(stoppingToken);
    }

    /// <inheritdoc/>
    public ConfiguredCancelableAsyncEnumerable<NoteEntity> ReadAllNotes(CancellationToken cancellationToken)
    {
        var notes = context.Notes
            .AsNoTracking()
            .AsAsyncEnumerable()
            .WithCancellation(cancellationToken);

        return notes;
    }

    /// <inheritdoc/>
    public async Task<string?> ReadNoteTitle(int noteId, CancellationToken cancellationToken)
    {
        var title = await context.Notes
            .Where(noteEntity => noteEntity.NoteId == noteId)
            .Select(noteEntity => noteEntity.Title)
            .FirstOrDefaultAsync(cancellationToken);

        return title;
    }

    /// <inheritdoc/>
    public async Task<List<int>> ReadTaggedNotesIds(IEnumerable<int> checkedTags, CancellationToken cancellationToken)
    {
        // TODO: определить какой метод лучше
        // IQueryable<int> songsCollection = database.GenreText//
        //    .Where(s => chosenOnes.Contains(s.GenreInGenreText.GenreID))
        //    .Select(s => s.TextInGenreText.TextID);

        var noteIds = await context.Notes
            .Where(note => note.RelationEntityReference!.Any(relation => checkedTags.Contains(relation.TagId)))
            .Select(note => note.NoteId)
            .ToListAsync(cancellationToken);

        return noteIds;
    }

    /// <inheritdoc/>
    public async Task<List<CatalogItemDto>> ReadCatalogPage(int pageNumber, int pageSize,
        CancellationToken cancellationToken)
    {
        var catalogPages = await context.Notes
            .OrderBy(note => note.Title)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(note => new CatalogItemDto { Title = note.Title!, NoteId = note.NoteId })
            .ToListAsync(cancellationToken);

        return catalogPages;
    }

    /// <inheritdoc/>
    public async Task<TextResultDto?> ReadNote(int noteId, CancellationToken cancellationToken)
    {
        var note = await context.Notes
            .Where(note => note.NoteId == noteId)
            .Select(note => new TextResultDto { Text = note.Text!, Title = note.Title! })
            .FirstOrDefaultAsync(cancellationToken);

        return note;
    }

    /// <inheritdoc/>
    public async Task<List<int>> ReadNoteTagIds(int noteId, CancellationToken cancellationToken)
    {
        var tagIds = await context.TagsToNotesRelation
            .Where(relation => relation.NoteInRelationEntity!.NoteId == noteId)
            .Select(relation => relation.TagId)
            .ToListAsync(cancellationToken);

        return tagIds;
    }

    /// <inheritdoc/>
    public async Task<UserEntity?> GetUser(CredentialsRequestDto credentialsRequest,
        CancellationToken cancellationToken)
    {
        return await context.Users.FirstOrDefaultAsync(user =>
            user.Email == credentialsRequest.Email && user.Password == credentialsRequest.Password, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<int> ReadNotesCount(CancellationToken cancellationToken)
    {
        return await context.Notes.CountAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<List<string>> ReadEnrichedTagList(CancellationToken cancellationToken)
    {
        var tagList = await context.Tags
            // todo: [?] заменить сортировку на корректный индекс в бд
            .OrderBy(tag => tag.TagId)
            .Select(tag => new EnrichedTagList(tag.Tag!, tag.RelationEntityReference!.Count))
            .ToListAsync(cancellationToken);

        return tagList.Select(tagAndAmount =>
                tagAndAmount.RelationEntityReferenceCount > 0
                    ? tagAndAmount.Tag + ": " + tagAndAmount.RelationEntityReferenceCount
                    : tagAndAmount.Tag)
            .ToList();
    }

    /// <inheritdoc/>
    public async Task UpdateNote(IEnumerable<int> initialTags, NoteRequestDto noteRequest,
        CancellationToken stoppingToken)
    {
        var forAddition = noteRequest.CheckedTags!.ToHashSet();

        var forDelete = initialTags.ToHashSet();

        var except = forAddition.Intersect(forDelete).ToList();

        forAddition.ExceptWith(except);

        forDelete.ExceptWith(except);

        // непонятно, какой кейс закрывает проверка именно первого тега
        await ThrowIfFirstTagExists(noteRequest.NoteIdExchange, forAddition, stoppingToken);

        // ID тегов и номера кнопок с фронта совпадают
        await using var transaction = await context.Database.BeginTransactionAsync(stoppingToken);

        var processedNote =
            await context.Notes.FindAsync([noteRequest.NoteIdExchange], cancellationToken: stoppingToken);

        if (processedNote == null)
        {
            throw new RsseInvalidDataException($"[{nameof(UpdateNote)}] null note entity");
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
                    }), stoppingToken);

        await context.SaveChangesAsync(stoppingToken);

        await transaction.CommitAsync(_noneToken);
    }

    /// <inheritdoc/>
    public async Task UpdateCredos(UpdateCredosRequestDto credosRequest, CancellationToken cancellationToken)
    {
        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);

        var processed = await context.Users.FirstOrDefaultAsync(userEntity =>
            userEntity.Email == credosRequest.OldCredos.Email &&
            userEntity.Password == credosRequest.OldCredos.Password, cancellationToken);
        if (processed == null)
            throw new RsseInvalidDataException(
                $"[{nameof(UpdateCredos)}] credos '{credosRequest.OldCredos.Email}:{credosRequest.OldCredos.Password}' are invalid");
        processed.Email = credosRequest.NewCredos.Email;
        processed.Password = credosRequest.NewCredos.Password;
        context.Users.Update(processed);
        await context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(_noneToken);
    }

    /// <inheritdoc/>
    public async Task<int> CreateNote(NoteRequestDto noteRequest, CancellationToken stoppingToken)
    {
        if (await VerifyTitleNotExists(noteRequest.Title!, stoppingToken) == false)
        {
            return noteRequest.NoteIdExchange;
        }

        // ReSharper disable once RedundantAssignment
        var createdId = 0;
        await using var transaction = await context.Database.BeginTransactionAsync(stoppingToken);

        var forAddition = new NoteEntity { Title = noteRequest.Title, Text = noteRequest.Text };

        await context.Notes.AddAsync(forAddition, stoppingToken);

        await context.SaveChangesAsync(stoppingToken);

        await context.TagsToNotesRelation
            .AddRangeAsync(noteRequest.CheckedTags!
                .Select(id =>
                    new TagsToNotesEntity
                    {
                        NoteId = forAddition.NoteId,
                        TagId = id
                    }), stoppingToken);

        await context.SaveChangesAsync(stoppingToken);

        await transaction.CommitAsync(_noneToken);

        createdId = forAddition.NoteId;

        return createdId;
    }

    /// <inheritdoc/>
    public async Task<int> DeleteNote(int noteId, CancellationToken stoppingToken)
    {
        await using var transaction = await context.Database.BeginTransactionAsync(stoppingToken);

        var deletedEntries = 0;

        var processedNote = await context.Notes.FindAsync([noteId], stoppingToken);

        if (processedNote == null)
        {
            return deletedEntries;
        }

        context.Notes.Remove(processedNote);

        deletedEntries = await context.SaveChangesAsync(stoppingToken);

        await transaction.CommitAsync(_noneToken);

        return deletedEntries;
    }

    private async Task<bool> VerifyTitleNotExists(string title, CancellationToken ct)
    {
        return !await context.Notes.AnyAsync(entity => entity.Title == title, ct);
    }

    private async Task ThrowIfFirstTagExists(int noteId, HashSet<int> forAddition, CancellationToken ct)
    {
        if (forAddition.Count == 0)
        {
            return;
        }

        var firstTag = forAddition.First();
        if (await context.TagsToNotesRelation.AnyAsync(relation =>
                relation.NoteId == noteId && relation.TagId == firstTag, ct))
        {
            throw new RsseDataExistsException($"[{nameof(ThrowIfFirstTagExists)}] tags exists error");
        }
    }
}
