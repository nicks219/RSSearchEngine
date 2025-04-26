using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SearchEngine.Data.Context;
using SearchEngine.Data.Dto;
using SearchEngine.Data.Entities;
using SearchEngine.Data.Repository.Contracts;
using SearchEngine.Data.Repository.Exceptions;

namespace SearchEngine.Data.Repository;

/// <summary>
/// Репозиторий для доступа к двум базам данных на время миграции (читаем из одной, пишем в обе).
/// </summary>
/// <param name="reader">бд для чтения</param>
/// <param name="writerPrimary">первая бд для записи</param>
/// <param name="writerSecondary">вторая бд для записи</param>
public class MirrorRepository(
    IOptionsSnapshot<DatabaseOptions> options,
    CatalogRepository<NpgsqlCatalogContext> reader,
    CatalogRepository<MysqlCatalogContext> writerPrimary,
    CatalogRepository<NpgsqlCatalogContext> writerSecondary)
    : IDataRepository
{
    // выбор контеста для чтения, завязан на резолве контекстов в конструкторах, добавлена compile-time проверка
    private readonly IDataRepository _reader = options.Value.ReaderContext switch
    {
        DatabaseType.Postgres => reader as CatalogRepository<NpgsqlCatalogContext>,
        DatabaseType.MySql => writerPrimary as CatalogRepository<MysqlCatalogContext>,
        _ => reader
    };
    private readonly IDataRepository _writerPrimary = writerPrimary;
    private readonly IDataRepository _writerSecondary = writerSecondary;

    public void Dispose()
    {
        _reader.Dispose();
        _writerPrimary.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        await _reader.DisposeAsync().ConfigureAwait(false);
        await _writerPrimary.DisposeAsync().ConfigureAwait(false);
    }

    public BaseCatalogContext? GetReaderContext() => _reader.GetReaderContext();

    public BaseCatalogContext? GetPrimaryWriterContext() => _writerPrimary.GetPrimaryWriterContext();

    // завязан на резолве контекстов в конструкторах, добавлена compile-time проверка
    public async Task CopyDbFromMysqlToNpgsql()
    {
        // todo: какое время жизни контекста? блокировать остальные операции с контекстом и выполнять данную только по завершению остальных?
        var mysqlCatalogContext = (writerPrimary as CatalogRepository<MysqlCatalogContext>).GetReaderContext();
        var npgsqlCatalogContext = (writerSecondary as CatalogRepository<NpgsqlCatalogContext>).GetReaderContext();
        if (mysqlCatalogContext == null || npgsqlCatalogContext == null)
            throw new InvalidOperationException($"[Warning] {nameof(CopyDbFromMysqlToNpgsql)} | null context(s).");

        // AddRangeAsync вместе с таблицей Notes подхватит селектнутые отношения, на это поведение нельзя полагаться
        var notes = mysqlCatalogContext.Notes!.Select(note => note).ToList();
        var tags = mysqlCatalogContext.Tags!.Select(tag => tag).ToList();
        var tagsToNotes = mysqlCatalogContext.TagsToNotesRelation!.Select(relation => relation).ToList();

        var users = mysqlCatalogContext.Users!.Select(user => user).ToList();

        await using var transaction = await npgsqlCatalogContext.Database.BeginTransactionAsync();

        try
        {
            // notes, tags, relations:
            await npgsqlCatalogContext.Notes!.AddRangeAsync(notes);
            await npgsqlCatalogContext.Tags!.AddRangeAsync(tags);
            await npgsqlCatalogContext.TagsToNotesRelation!.AddRangeAsync(tagsToNotes);

            // users:
            await npgsqlCatalogContext.Users!.ExecuteDeleteAsync();
            await npgsqlCatalogContext.Users!.AddRangeAsync(users);

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

    // <summary/> выставить актуальные значения ключей для postgres
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

    public async Task<int> CreateNote(NoteDto note)
    {
        _ = await _writerPrimary.CreateNote(note);
        // secondary: CatalogRepository<NpgsqlCatalogContext>
        var secondary = await _writerSecondary.CreateNote(note);
        return secondary;
    }

    public IQueryable<Tuple<string, string>> ReadNote(int noteId) => _reader.ReadNote(noteId);

    public Task UpdateNote(IEnumerable<int> initialTags, NoteDto note)
    {
        // todo: можно сразу принимать IList
        var enumerable = initialTags.ToList();
        return Task.WhenAll(_writerPrimary.UpdateNote(enumerable, note), _writerSecondary.UpdateNote(enumerable, note));
    }

    public async Task<int> DeleteNote(int noteId)
    {
        _ = await _writerPrimary.DeleteNote(noteId);
        // secondary: CatalogRepository<NpgsqlCatalogContext>
        var secondary = await _writerSecondary.DeleteNote(noteId);
        return secondary;
    }

    public Task CreateTagIfNotExists(string tag)
    {
        return Task.WhenAll(_writerPrimary.CreateTagIfNotExists(tag), _writerSecondary.CreateTagIfNotExists(tag));
    }

    public Task<List<string>> ReadStructuredTagList() => _reader.ReadStructuredTagList();

    public Task<int> ReadNotesCount() => _reader.ReadNotesCount();

    public IQueryable<NoteEntity> ReadAllNotes() => _reader.ReadAllNotes();

    public IQueryable<int> ReadTaggedNotes(IEnumerable<int> checkedTags) => _reader.ReadTaggedNotes(checkedTags);

    public string ReadNoteTitle(int noteId) => _reader.ReadNoteTitle(noteId);

    public int ReadNoteId(string noteTitle) => _reader.ReadNoteId(noteTitle);

    public IQueryable<int> ReadNoteTags(int noteId) => _reader.ReadNoteTags(noteId);

    public IQueryable<Tuple<string, int>> ReadCatalogPage(int pageNumber, int pageSize) => _reader.ReadCatalogPage(pageNumber, pageSize);

    public Task<UserEntity?> GetUser(LoginDto login) => _reader.GetUser(login);
}
