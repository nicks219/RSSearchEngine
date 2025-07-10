using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Rsse.Domain.Data.Contracts;
using Rsse.Domain.Data.Dto;
using Rsse.Domain.Data.Entities;
using Rsse.Domain.Exceptions;
using Rsse.Domain.Service.Contracts;
using Rsse.Domain.Service.Mapping;
using RsseEngine.Dto;
using RsseEngine.Indexes;
using RsseEngine.SearchType;
using RsseEngine.Tokenizer.Common;
using RsseEngine.Tokenizer.Contracts;

namespace RsseEngine.Service;

/// <summary>
/// Сервис токенайзера (в т.ч инициализация и поиск).
/// </summary>
public sealed class TokenizerServiceCore : ITokenizerServiceCore, IAlgorithmConfigurable
{
    private TokenizerLock TokenizerLock { get; } = new();
    private readonly SearchEngineManager _searchEngineManager;
    private ExtendedSearchType _extendedSearchType;
    private ReducedSearchType _reducedSearchType;

    /// <summary>
    /// Флаг инициалицации сервиса токенайзера.
    /// </summary>
    private volatile bool _isActivated;

    /// <summary>
    /// Создать токенайзер.
    /// </summary>
    /// <param name="enableTempStoragePool">Пул активирован.</param>
    /// <param name="extendedSearchType">Тип оптимизации расширенного алгоритма поиска.</param>
    /// <param name="reducedSearchType">Тип оптимизации сокращенного алгоритма поиска.</param>
    public TokenizerServiceCore(
        bool enableTempStoragePool,
        ExtendedSearchType extendedSearchType = ExtendedSearchType.Legacy,
        ReducedSearchType reducedSearchType = ReducedSearchType.Legacy)
    {
        _searchEngineManager = new SearchEngineManager(enableTempStoragePool, true);
        _extendedSearchType = extendedSearchType;
        _reducedSearchType = reducedSearchType;
    }

    // Используется для тестов.
    internal DirectIndex GetDirectIndex() => _searchEngineManager.GetDirectIndex();

    /// <inheritdoc/>
    public async Task<bool> DeleteAsync(int id, CancellationToken stoppingToken)
    {
        using var __ = await TokenizerLock.AcquireExclusiveLockAsync(stoppingToken);

        var documentId = new DocumentId(id);

        var removed = _searchEngineManager.TryRemove(documentId);

        return removed;
    }

    /// <inheritdoc/>
    public async Task<bool> CreateAsync(int id, TextRequestDto note, CancellationToken stoppingToken)
    {
        using var _ = await TokenizerLock.AcquireExclusiveLockAsync(stoppingToken);

        var createdTokenLine = CreateTokensLine(note);
        var docId = new DocumentId(id);

        var created = _searchEngineManager.TryAdd(docId, createdTokenLine);

        return created;
    }

    /// <inheritdoc/>
    public async Task<bool> UpdateAsync(int id, TextRequestDto note, CancellationToken stoppingToken)
    {
        using var _ = await TokenizerLock.AcquireExclusiveLockAsync(stoppingToken);

        var updatedTokenLine = CreateTokensLine(note);
        var docId = new DocumentId(id);

        var updated = _searchEngineManager.TryUpdate(docId, updatedTokenLine);

        // в данной реализации ошибки получения и обновления не разделяются
        return updated;
    }

    /// <inheritdoc/>
    public async Task<int> InitializeAsync(IDataProvider<NoteEntity> dataProvider, CancellationToken stoppingToken)
    {
        // Инициализация вызывается не только не старте сервиса и её следует разграничить с остальными меняющими данные операций.
        using var _ = await TokenizerLock.AcquireExclusiveLockAsync(stoppingToken);

        try
        {
            _searchEngineManager.Clear();

            // todo: подумать, как избавиться от загрузки всех записей из таблицы
            var notes = dataProvider.GetDataAsync().WithCancellation(stoppingToken);

            // todo: на старте сервиса при отсутствии коннекта до баз данных перечисление спамит логами с исключениями
            await foreach (var note in notes)
            {
                if (stoppingToken.IsCancellationRequested) throw new OperationCanceledException(nameof(InitializeAsync));

                var requestNote = note.MapToDto();
                var tokenLine = CreateTokensLine(requestNote);
                var noteDocId = new DocumentId(note.NoteId);

                if (!_searchEngineManager.TryAdd(noteDocId, tokenLine))
                {
                    throw new RsseTokenizerException($"[{nameof(TokenizerServiceCore)}] vector initialization error");
                }
            }
        }
        catch (Exception ex)
        {
            throw new RsseTokenizerException($"[{nameof(TokenizerServiceCore)}] initialization system error | " +
                                             $"'{ex.Source}' | '{ex.Message}'");
        }

        var count = _searchEngineManager.DirectIndexCount;

        _isActivated = true;

        return count;
    }

    /// <inheritdoc/>
    public async Task<bool> WaitWarmUpAsync(CancellationToken timeoutToken)
    {
        await TokenizerLock.SyncOnLockAsync(timeoutToken);

        return _isActivated;
    }

    /// <inheritdoc/>
    public bool IsInitialized() => _isActivated;

    /// <inheritdoc/>
    public Dictionary<DocumentId, double> ComputeComplianceIndices(string text, CancellationToken cancellationToken)
    {
        var metricsCalculator = new MetricsCalculator();

        var extendedSearchVector = _searchEngineManager.ExtendedTokenizer.TokenizeText(text);

        if (extendedSearchVector.Count == 0)
        {
            // заметки вида "123 456" не ищем, так как получим весь каталог
            return metricsCalculator.ComplianceMetrics;
        }

        _searchEngineManager.FindExtended(_extendedSearchType,
            extendedSearchVector, metricsCalculator, cancellationToken);

        if (!metricsCalculator.ContinueSearching)
        {
            return metricsCalculator.ComplianceMetrics;
        }

        var reducedSearchVector = _searchEngineManager.ReducedTokenizer.TokenizeText(text);

        if (reducedSearchVector.Count == 0)
        {
            // песни вида "123 456" не ищем, так как получим весь каталог
            return metricsCalculator.ComplianceMetrics;
        }

        _searchEngineManager.FindReduced(_reducedSearchType,
            reducedSearchVector, metricsCalculator, cancellationToken);

        return metricsCalculator.ComplianceMetrics;
    }

    /// <inheritdoc/>
    public Dictionary<DocumentId, double> ComputeComplianceIndexExtended(string text, CancellationToken cancellationToken)
    {
        var metricsCalculator = new MetricsCalculator();

        var extendedSearchVector = _searchEngineManager.ExtendedTokenizer.TokenizeText(text);

        if (extendedSearchVector.Count == 0)
        {
            // заметки вида "123 456" не ищем, так как получим весь каталог
            return metricsCalculator.ComplianceMetrics;
        }

        _searchEngineManager.FindExtended(_extendedSearchType,
            extendedSearchVector, metricsCalculator, cancellationToken);

        return metricsCalculator.ComplianceMetrics;
    }

    /// <inheritdoc/>
    public Dictionary<DocumentId, double> ComputeComplianceIndexReduced(string text, CancellationToken cancellationToken)
    {
        var metricsCalculator = new MetricsCalculator();

        var reducedSearchVector = _searchEngineManager.ReducedTokenizer.TokenizeText(text);

        if (reducedSearchVector.Count == 0)
        {
            // песни вида "123 456" не ищем, так как получим весь каталог
            return metricsCalculator.ComplianceMetrics;
        }

        _searchEngineManager.FindReduced(_reducedSearchType,
            reducedSearchVector, metricsCalculator, cancellationToken);

        return metricsCalculator.ComplianceMetrics;
    }

    /// <summary>
    /// Создать два вектора токенов для заметки.
    /// </summary>
    /// <param name="note">Текстовая нагрузка заметки.</param>
    /// <returns>Векторы на базе двух разных эталонных наборов.</returns>
    private TokenLine CreateTokensLine(TextRequestDto note)
    {
        if (note.Text == null || note.Title == null)
            throw new ArgumentNullException(nameof(note), "Request text or title should not be null.");

        // расширенная эталонная последовательность:
        var extendedTokenLine = _searchEngineManager.ExtendedTokenizer.TokenizeText(note.Text, " ", note.Title);

        // урезанная эталонная последовательность:
        var reducedTokenLine = _searchEngineManager.ReducedTokenizer.TokenizeText(note.Text, " ", note.Title);

        return new TokenLine(Extended: extendedTokenLine, Reduced: reducedTokenLine);
    }

    /// <summary>
    /// Освобождаем ресурсы блокировки.
    /// </summary>
    public void Dispose() => TokenizerLock.Dispose();

    /// <summary>
    /// Сконфигурировать алгоритмы поиска.
    /// </summary>
    /// <param name="extendedSearchType">Алгоритм четкого поиска.</param>
    /// <param name="reducedSearchType">Алгоритм нечеткого поиска.</param>
    internal void ConfigureSearchEngine(ExtendedSearchType extendedSearchType, ReducedSearchType reducedSearchType)
    {
        _extendedSearchType = extendedSearchType;
        _reducedSearchType = reducedSearchType;
    }
}
