using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using SearchEngine.Data.Configuration;
using SearchEngine.Data.Contracts;
using SearchEngine.Data.Dto;
using SearchEngine.Data.Entities;
using SearchEngine.Infrastructure.Context;

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
        DatabaseType.Postgres => reader as CatalogRepository<NpgsqlCatalogContext>,
        DatabaseType.MySql => writerPrimary as CatalogRepository<MysqlCatalogContext>,
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
