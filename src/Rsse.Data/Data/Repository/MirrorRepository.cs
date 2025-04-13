using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using SearchEngine.Data.Context;
using SearchEngine.Data.Dto;
using SearchEngine.Data.Entities;
using SearchEngine.Data.Repository.Contracts;
using SearchEngine.Data.Repository.Exceptions;

namespace SearchEngine.Data.Repository;

public class DatabaseOptions
{
    /// <summary>
    /// Выбор бд для чтения.
    /// </summary>
    public DatabaseType MainContext { get; set; }
}

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
    // завязан на резолве контекстов в конструкторах, добавлена compile-time проверка
    private readonly IDataRepository _reader = options.Value.MainContext switch
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

    // завязан на резолве контекстов в конструкторах, добавлена compile-time проверка
    public async Task CopyDbFromMysqlToNpgsql()
    {
        // todo: какое время жизни контекста? блокировать остальные операции с контекстом и выполнять данную только по завершению остальных?
        var mysqlCatalogContext = (writerPrimary as CatalogRepository<MysqlCatalogContext>).GetMainContext();
        var npgsqlCatalogContext = (writerSecondary as CatalogRepository<NpgsqlCatalogContext>).GetMainContext();
        if (mysqlCatalogContext == null || npgsqlCatalogContext == null)
            throw new InvalidOperationException($"[Warning] {nameof(CopyDbFromMysqlToNpgsql)} | null context(s).");

        // AddRangeAsync вместе с таблицей Notes подхватит селектнутые отношения:
        var notes = mysqlCatalogContext.Notes!.Select(note => note).ToList();
        _ = mysqlCatalogContext.TagsToNotesRelation!.Select(relation => relation).ToList();
        _ = mysqlCatalogContext.Tags!.Select(tag => tag).ToList();
        var users = mysqlCatalogContext.Users!.Select(user => user).ToList();

        await using var transaction = await npgsqlCatalogContext.Database.BeginTransactionAsync();

        try
        {
            // notes, tags, relations:
            await npgsqlCatalogContext.Notes!.AddRangeAsync(notes);

            await npgsqlCatalogContext.Users!.AddRangeAsync(users);

            await npgsqlCatalogContext.SaveChangesAsync();

            await transaction.CommitAsync();
        }
        catch (DataExistsException)
        {
            await transaction.RollbackAsync();
        }
        catch (Exception ex)
        {
            // Include Error Detail:
            await transaction.RollbackAsync();
            Console.WriteLine(ex.Message);
            throw new Exception($"[{nameof(CreateNote)}: Repo]", ex);
        }
    }

    public BaseCatalogContext? GetMainContext() => _reader.GetMainContext();

    public BaseCatalogContext? GetAdditionalContext() => _writerPrimary.GetMainContext();

    public async Task<int> CreateNote(NoteDto note)
    {
        var primary = await _writerPrimary.CreateNote(note);
        var secondary = await _writerSecondary.CreateNote(note);
        return primary + secondary;
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
        var primary = await _writerPrimary.DeleteNote(noteId);
        var secondary = await _writerSecondary.DeleteNote(noteId);
        return primary + secondary;
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

    public Task CreateTagIfNotExists(string tag)
    {
        return Task.WhenAll(_writerPrimary.CreateTagIfNotExists(tag), _writerSecondary.CreateTagIfNotExists(tag));
    }
}
