using System.Collections.Generic;
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
    public async Task<int> CreateNote(NoteRequestDto noteRequest, CancellationToken stoppingToken)
    {
        _ = await _writerPrimary.CreateNote(noteRequest, stoppingToken);
        var secondary = await _writerSecondary.CreateNote(noteRequest, stoppingToken);
        return secondary;
    }

    /// <inheritdoc/>
    public Task<TextResultDto?> ReadNote(int noteId, CancellationToken cancellationToken) =>
        _reader.ReadNote(noteId, cancellationToken);

    /// <inheritdoc/>
    public Task UpdateNote(NoteRequestDto noteRequest, CancellationToken stoppingToken)
    {
        return Task.WhenAll(_writerPrimary.UpdateNote(noteRequest, stoppingToken),
            _writerSecondary.UpdateNote(noteRequest, stoppingToken));
    }

    /// <inheritdoc/>
    public Task UpdateCredos(UpdateCredosRequestDto credosRequest, CancellationToken cancellationToken)
    {
        return Task.WhenAll(_writerPrimary.UpdateCredos(credosRequest, cancellationToken),
            _writerSecondary.UpdateCredos(credosRequest, cancellationToken));
    }

    /// <inheritdoc/>
    public async Task<int> DeleteNote(int noteId, CancellationToken stoppingToken)
    {
        _ = await _writerPrimary.DeleteNote(noteId, stoppingToken);
        var secondary = await _writerSecondary.DeleteNote(noteId, stoppingToken);
        return secondary;
    }

    /// <inheritdoc/>
    public Task CreateTagIfNotExists(string tag, CancellationToken stoppingToken)
    {
        return Task.WhenAll(_writerPrimary.CreateTagIfNotExists(tag, stoppingToken),
            _writerSecondary.CreateTagIfNotExists(tag, stoppingToken));
    }

    /// <inheritdoc/>
    public Task<List<TagResultDto>> ReadTags(CancellationToken cancellationToken) =>
        _reader.ReadTags(cancellationToken);

    /// <inheritdoc/>
    public Task<List<TagMarkedResultDto>> ReadMarkedTags(int nodeId, CancellationToken cancellationToken) =>
        _reader.ReadMarkedTags(nodeId, cancellationToken);

    /// <inheritdoc/>
    public Task<int> ReadNotesCount(CancellationToken cancellationToken) => _reader.ReadNotesCount(cancellationToken);

    /// <inheritdoc/>
    public ConfiguredCancelableAsyncEnumerable<NoteEntity> ReadAllNotes(CancellationToken cancellationToken) =>
        _reader.ReadAllNotes(cancellationToken);

    /// <inheritdoc/>
    public Task<List<int>> ReadTaggedNotesIds(IEnumerable<int> checkedTags, CancellationToken cancellationToken) =>
        _reader.ReadTaggedNotesIds(checkedTags, cancellationToken);

    /// <inheritdoc/>
    public Task<NoteEntity?> GetRandomNoteOrDefault(IEnumerable<int> checkedTags, CancellationToken cancellationToken) =>
        _reader.GetRandomNoteOrDefault(checkedTags, cancellationToken);

    /// <inheritdoc/>
    public Task<string?> ReadNoteTitle(int noteId, CancellationToken cancellationToken) =>
        _reader.ReadNoteTitle(noteId, cancellationToken);

    /// <inheritdoc/>
    public Task<List<CatalogItemDto>>
        ReadCatalogPage(int pageNumber, int pageSize, CancellationToken cancellationToken) =>
        _reader.ReadCatalogPage(pageNumber, pageSize, cancellationToken);

    /// <inheritdoc/>
    public Task<UserEntity?> GetUser(CredentialsRequestDto credentialsRequest, CancellationToken cancellationToken) =>
        _reader.GetUser(credentialsRequest, cancellationToken);
}
