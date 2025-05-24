using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
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

    /// <inheritdoc/>
    public async Task<int> CreateNote(NoteRequestDto noteRequest, CancellationToken ct)
    {
        _ = await _writerPrimary.CreateNote(noteRequest, ct);
        var secondary = await _writerSecondary.CreateNote(noteRequest, ct);
        return secondary;
    }

    /// <inheritdoc/>
    public Task<TextResultDto?> ReadNote(int noteId, CancellationToken ct) => _reader.ReadNote(noteId, ct);

    /// <inheritdoc/>
    public Task UpdateNote(IEnumerable<int> initialTags, NoteRequestDto noteRequest, CancellationToken ct)
    {
        // todo: можно сразу принимать IList
        var enumerable = initialTags.ToList();
        return Task.WhenAll(_writerPrimary.UpdateNote(enumerable, noteRequest, ct),
            _writerSecondary.UpdateNote(enumerable, noteRequest, ct));
    }

    /// <inheritdoc/>
    public Task UpdateCredos(UpdateCredosRequestDto credosRequest, CancellationToken ct)
    {
        return Task.WhenAll(_writerPrimary.UpdateCredos(credosRequest, ct),
            _writerSecondary.UpdateCredos(credosRequest, ct));
    }

    /// <inheritdoc/>
    public async Task<int> DeleteNote(int noteId, CancellationToken ct)
    {
        _ = await _writerPrimary.DeleteNote(noteId, ct);
        var secondary = await _writerSecondary.DeleteNote(noteId, ct);
        return secondary;
    }

    /// <inheritdoc/>
    public Task CreateTagIfNotExists(string tag, CancellationToken ct)
    {
        return Task.WhenAll(_writerPrimary.CreateTagIfNotExists(tag, ct),
            _writerSecondary.CreateTagIfNotExists(tag, ct));
    }

    /// <inheritdoc/>
    public Task<List<string>> ReadEnrichedTagList(CancellationToken ct) => _reader.ReadEnrichedTagList(ct);

    /// <inheritdoc/>
    public Task<int> ReadNotesCount(CancellationToken ct) => _reader.ReadNotesCount(ct);

    /// <inheritdoc/>
    public ConfiguredCancelableAsyncEnumerable<NoteEntity> ReadAllNotes(CancellationToken ct) =>
        _reader.ReadAllNotes(ct);

    /// <inheritdoc/>
    public Task<List<int>> ReadTaggedNotesIds(IEnumerable<int> checkedTags, CancellationToken ct) =>
        _reader.ReadTaggedNotesIds(checkedTags, ct);

    /// <inheritdoc/>
    public Task<string?> ReadNoteTitle(int noteId, CancellationToken ct) => _reader.ReadNoteTitle(noteId, ct);

    /// <inheritdoc/>
    public Task<List<int>> ReadNoteTagIds(int noteId, CancellationToken ct) => _reader.ReadNoteTagIds(noteId, ct);

    /// <inheritdoc/>
    public Task<List<CatalogItemDto>> ReadCatalogPage(int pageNumber, int pageSize, CancellationToken ct) =>
        _reader.ReadCatalogPage(pageNumber, pageSize, ct);

    /// <inheritdoc/>
    public Task<UserEntity?> GetUser(CredentialsRequestDto credentialsRequest, CancellationToken ct) =>
        _reader.GetUser(credentialsRequest, ct);
}
