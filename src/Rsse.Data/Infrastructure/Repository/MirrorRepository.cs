using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SearchEngine.Domain.Configuration;
using SearchEngine.Domain.Contracts;
using SearchEngine.Domain.Dto;
using SearchEngine.Domain.Entities;
using SearchEngine.Infrastructure.Context;
using SearchEngine.Infrastructure.Repository.Exceptions;

namespace SearchEngine.Infrastructure.Repository;

/// <summary>
/// Проксирующйи репозиторий доступа к двум базам данных на время миграции (читаем из одной бд, пишем в обе).
/// </summary>
/// <param name="reader">Репозиторий для чтения.</param>
/// <param name="writerPrimary">Первый репозиторий для записи.</param>
/// <param name="writerSecondary">Второй репозиторий для записи.</param>
public sealed class MirrorRepository(
    IOptionsSnapshot<DatabaseOptions> options,
    CatalogRepository<NpgsqlCatalogContext> reader,
    CatalogRepository<MysqlCatalogContext> writerPrimary,
    CatalogRepository<NpgsqlCatalogContext> writerSecondary)
    : IDataRepository
{
    // выбор контеста для чтения, завязан на резолве контекстов в конструкторах, добавлена compile-time проверка.
    private readonly IDataRepository _reader = options.Value.ReaderContext switch
    {
        MigratorType.Postgres => reader as CatalogRepository<NpgsqlCatalogContext>,
        MigratorType.MySql => writerPrimary as CatalogRepository<MysqlCatalogContext>,
        _ => reader
    };
    private readonly IDataRepository _writerPrimary = writerPrimary;
    private readonly IDataRepository _writerSecondary = writerSecondary;

    // todo: на старте отрабатывает четыре раза, см. changelog
    /// <inheritdoc/>
    public void Dispose()
    {
        _reader.Dispose();
        _writerPrimary.Dispose();
    }

    // todo: на старте отрабатывает четыре раза, см. changelog
    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        await _reader.DisposeAsync().ConfigureAwait(false);
        await _writerPrimary.DisposeAsync().ConfigureAwait(false);
    }

    // todo: удалить после перехода на Postgres
    /// <inheritdoc/>
    public BaseCatalogContext? GetReaderContext() => _reader.GetReaderContext();
    /// <inheritdoc/>
    public BaseCatalogContext? GetPrimaryWriterContext() => _writerPrimary.GetPrimaryWriterContext();
    /// <inheritdoc/>
    /// // завязан на резолве контекстов в конструкторах, добавлена compile-time проверка
    public async Task CopyDbFromMysqlToNpgsql()
    {
        // todo: какое время жизни контекста? блокировать остальные операции с контекстом и выполнять данную только по завершению остальных?
        var mysqlCatalogContext = (writerPrimary as CatalogRepository<MysqlCatalogContext>).GetReaderContext();
        var npgsqlCatalogContext = (writerSecondary as CatalogRepository<NpgsqlCatalogContext>).GetReaderContext();
        if (mysqlCatalogContext == null || npgsqlCatalogContext == null)
            throw new InvalidOperationException($"[Warning] {nameof(CopyDbFromMysqlToNpgsql)} | null context(s).");

        // пересоздаём базу перед копированием данных
        await npgsqlCatalogContext.Database.EnsureDeletedAsync();
        await npgsqlCatalogContext.Database.EnsureCreatedAsync();

        // AddRangeAsync вместе с таблицей Notes подхватит селектнутые отношения, на это поведение нельзя полагаться
        var notes = mysqlCatalogContext.Notes.Select(note => note).ToList();
        var tags = mysqlCatalogContext.Tags.Select(tag => tag).ToList();
        var tagsToNotes = mysqlCatalogContext.TagsToNotesRelation.Select(relation => relation).ToList();

        var users = mysqlCatalogContext.Users.Select(user => user).ToList();

        await using var transaction = await npgsqlCatalogContext.Database.BeginTransactionAsync();

        try
        {
            // notes, tags, relations:
            await npgsqlCatalogContext.Notes.AddRangeAsync(notes);
            await npgsqlCatalogContext.Tags.AddRangeAsync(tags);
            await npgsqlCatalogContext.TagsToNotesRelation.AddRangeAsync(tagsToNotes);

            // users:
            await npgsqlCatalogContext.Users.ExecuteDeleteAsync();
            await npgsqlCatalogContext.Users.AddRangeAsync(users);

            await npgsqlCatalogContext.SaveChangesAsync();

            // мы заполнили значение ключей "вручную" и EF не изменил identity
            await PgSetVals(npgsqlCatalogContext);

            await transaction.CommitAsync();
        }
        catch (DataExistsException)
        {
            await transaction.RollbackAsync();
        }
        catch (Exception ex)
        {
            // include error detail:
            await transaction.RollbackAsync();
            Console.WriteLine(ex.Message);
            throw new Exception($"[{nameof(CreateNote)}: Repo]", ex);
        }
    }

    // <summary/> Выставить актуальные значения ключей для postgres.
    private static async Task PgSetVals(BaseCatalogContext dbContext)
    {
        if (dbContext.Database.ProviderName != "Npgsql.EntityFrameworkCore.PostgreSQL")
        {
            throw new NotSupportedException($"{nameof(PgSetVals)} | '{dbContext.Database.ProviderName}' provider is not supported.");
        }

        var noteRows = await dbContext.Database.ExecuteSqlRawAsync("""SELECT setval(pg_get_serial_sequence('"Note"', 'NoteId'),(SELECT MAX("NoteId") FROM "Note"));""");
        var tagRows = await dbContext.Database.ExecuteSqlRawAsync("""SELECT setval(pg_get_serial_sequence('"Tag"', 'TagId'),(SELECT MAX("TagId") FROM "Tag"));""");
        var userRows = await dbContext.Database.ExecuteSqlRawAsync("""SELECT setval(pg_get_serial_sequence('"Users"', 'Id'),(SELECT MAX("Id") FROM "Users"));""");
        Console.WriteLine($"repo set val | noteRows : {noteRows} | tagRows : {tagRows} | userRows : {userRows}");
    }

    /// <inheritdoc/>
    public async Task<int> CreateNote(NoteRequestDto noteRequest)
    {
        _ = await _writerPrimary.CreateNote(noteRequest);
        // secondary: CatalogRepository<NpgsqlCatalogContext>
        var secondary = await _writerSecondary.CreateNote(noteRequest);
        return secondary;
    }

    /// <inheritdoc/>
    public Task<TextResultDto?> ReadNote(int noteId) => _reader.ReadNote(noteId);

    /// <inheritdoc/>
    public Task UpdateNote(IEnumerable<int> initialTags, NoteRequestDto noteRequest)
    {
        // todo: можно сразу принимать IList
        var enumerable = initialTags.ToList();
        return Task.WhenAll(_writerPrimary.UpdateNote(enumerable, noteRequest), _writerSecondary.UpdateNote(enumerable, noteRequest));
    }

    /// <inheritdoc/>
    public Task UpdateCredos(UpdateCredosRequestDto credosRequest)
    {
        return Task.WhenAll(_writerPrimary.UpdateCredos(credosRequest), _writerSecondary.UpdateCredos(credosRequest));
    }

    /// <inheritdoc/>
    public async Task<int> DeleteNote(int noteId)
    {
        _ = await _writerPrimary.DeleteNote(noteId);
        // secondary: CatalogRepository<NpgsqlCatalogContext>
        var secondary = await _writerSecondary.DeleteNote(noteId);
        return secondary;
    }

    /// <inheritdoc/>
    public Task CreateTagIfNotExists(string tag)
    {
        return Task.WhenAll(_writerPrimary.CreateTagIfNotExists(tag), _writerSecondary.CreateTagIfNotExists(tag));
    }

    /// <inheritdoc/>
    public Task<List<string>> ReadEnrichedTagList() => _reader.ReadEnrichedTagList();

    /// <inheritdoc/>
    public Task<int> ReadNotesCount() => _reader.ReadNotesCount();

    /// <inheritdoc/>
    public IAsyncEnumerable<NoteEntity> ReadAllNotes() => _reader.ReadAllNotes();

    /// <inheritdoc/>
    public Task<List<int>> ReadTaggedNotesIds(IEnumerable<int> checkedTags) => _reader.ReadTaggedNotesIds(checkedTags);

    /// <inheritdoc/>
    public Task<string?> ReadNoteTitle(int noteId) => _reader.ReadNoteTitle(noteId);

    /// <inheritdoc/>
    public Task<List<int>> ReadNoteTagIds(int noteId) => _reader.ReadNoteTagIds(noteId);

    /// <inheritdoc/>
    public Task<List<CatalogItemDto>> ReadCatalogPage(int pageNumber, int pageSize) => _reader.ReadCatalogPage(pageNumber, pageSize);

    /// <inheritdoc/>
    public Task<UserEntity?> GetUser(CredentialsRequestDto credentialsRequest) => _reader.GetUser(credentialsRequest);
}
