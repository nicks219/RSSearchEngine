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
        throw new NotImplementedException();
    }

    public ValueTask DisposeAsync()
    {
        throw new NotImplementedException();
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

    public BaseCatalogContext? GetMainContext()
    {
        throw new NotImplementedException();
    }

    public BaseCatalogContext? GetAdditionalContext()
    {
        throw new NotImplementedException();
    }

    public Task<int> CreateNote(NoteDto note)
    {
        throw new NotImplementedException();
    }

    public IQueryable<Tuple<string, string>> ReadNote(int noteId)
    {
        throw new NotImplementedException();
    }

    public Task UpdateNote(IEnumerable<int> initialTags, NoteDto note)
    {
        throw new NotImplementedException();
    }

    public Task<int> DeleteNote(int noteId)
    {
        throw new NotImplementedException();
    }

    public Task<List<string>> ReadStructuredTagList()
    {
        throw new NotImplementedException();
    }

    public Task<int> ReadNotesCount()
    {
        throw new NotImplementedException();
    }

    public IQueryable<NoteEntity> ReadAllNotes()
    {
        throw new NotImplementedException();
    }

    public IQueryable<int> ReadTaggedNotes(IEnumerable<int> checkedTags)
    {
        throw new NotImplementedException();
    }

    public string ReadNoteTitle(int noteId)
    {
        throw new NotImplementedException();
    }

    public int ReadNoteId(string noteTitle)
    {
        throw new NotImplementedException();
    }

    public IQueryable<int> ReadNoteTags(int noteId)
    {
        throw new NotImplementedException();
    }

    public IQueryable<Tuple<string, int>> ReadCatalogPage(int pageNumber, int pageSize)
    {
        throw new NotImplementedException();
    }

    public Task<UserEntity?> GetUser(LoginDto login)
    {
        throw new NotImplementedException();
    }

    public Task CreateTagIfNotExists(string tag)
    {
        throw new NotImplementedException();
    }
}
