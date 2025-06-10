using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SearchEngine.Data.Contracts;
using SearchEngine.Data.Dto;
using SearchEngine.Data.Entities;
using SearchEngine.Exceptions;
using SearchEngine.Service.Mapping;
using SearchEngine.Service.Tokenizer.Contracts;
using SearchEngine.Service.Tokenizer.Dto;
using SearchEngine.Service.Tokenizer.SearchProcessor;
using SearchEngine.Service.Tokenizer.TokenizerProcessor;

namespace SearchEngine.Service.Tokenizer;

/// <summary>
/// Функционал поддержки токенайзера.
/// </summary>
public sealed class SearchEngineTokenizer : ISearchEngineTokenizer
{
    private TokenizerLock TokenizerLock { get; } = new();
    private readonly ConcurrentDictionary<DocId, TokenLine> _generalDirectIndex;
    private readonly GinHandler _ginExtended;
    private readonly GinHandler _ginReduced;
    private readonly SearchProcessorFactory _searchProcessorFactory;
    private readonly SearchType _searchType;

    private readonly ITokenizerProcessorFactory _tokenizerProcessorFactory;

    /// <summary>
    /// Флаг инициалицации токенайзера.
    /// </summary>
    private volatile bool _isActivated;

    /// <summary>
    /// Создать токенайзер.
    /// </summary>
    /// <param name="tokenizerProcessorFactory">Фабрика токенайзеров.</param>
    /// <param name="searchType">Тип оптимизации алгоритма поиска.</param>
    public SearchEngineTokenizer(
        ITokenizerProcessorFactory tokenizerProcessorFactory,
        SearchType searchType = SearchType.Original)
    {
        _generalDirectIndex = new ConcurrentDictionary<DocId, TokenLine>();
        _tokenizerProcessorFactory = tokenizerProcessorFactory;
        _searchType = searchType;

        _ginExtended = new GinHandler();
        _ginReduced = new GinHandler();
        _searchProcessorFactory = new SearchProcessorFactory(
            _ginExtended,
            _ginReduced,
            _generalDirectIndex,
            _tokenizerProcessorFactory,
            _searchType);
    }

    // Используется для тестов.
    internal ConcurrentDictionary<DocId, TokenLine> GetTokenLines() => _generalDirectIndex;

    /// <inheritdoc/>
    public async Task<bool> DeleteAsync(int id, CancellationToken stoppingToken)
    {
        using var __ = await TokenizerLock.AcquireExclusiveLockAsync(stoppingToken);
        var docId = new DocId(id);
        var removed = _generalDirectIndex.TryRemove(docId, out _);

        return removed;
    }

    /// <inheritdoc/>
    public async Task<bool> CreateAsync(int id, TextRequestDto note, CancellationToken stoppingToken)
    {
        using var _ = await TokenizerLock.AcquireExclusiveLockAsync(stoppingToken);

        var createdTokenLine = CreateTokensLine(_tokenizerProcessorFactory, note);

        var docId = new DocId(id);
        var created = _generalDirectIndex.TryAdd(docId, createdTokenLine);

        return created;
    }

    /// <inheritdoc/>
    public async Task<bool> UpdateAsync(int id, TextRequestDto note, CancellationToken stoppingToken)
    {
        using var _ = await TokenizerLock.AcquireExclusiveLockAsync(stoppingToken);

        var updatedTokenLine = CreateTokensLine(_tokenizerProcessorFactory, note);

        var docId = new DocId(id);
        if (!_generalDirectIndex.TryGetValue(docId, out var existedLine))
        {
            return false;
        }

        var updated = _generalDirectIndex.TryUpdate(docId, updatedTokenLine, existedLine);
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
            _generalDirectIndex.Clear();

            // todo: подумать, как избавиться от загрузки всех записей из таблицы
            var notes = dataProvider.GetDataAsync().WithCancellation(stoppingToken);

            // todo: на старте сервиса при отсутствии коннекта до баз данных перечисление спамит логами с исключениями
            await foreach (var note in notes)
            {
                if (stoppingToken.IsCancellationRequested) throw new OperationCanceledException(nameof(InitializeAsync));

                var requestNote = note.MapToDto();

                var tokenLine = CreateTokensLine(_tokenizerProcessorFactory, requestNote);

                if (_searchType != SearchType.Original)
                {
                    var noteDocId = new DocId(note.NoteId);
                    var extendedVector = tokenLine.Extended;
                    var reducedVector = tokenLine.Reduced;
                    _ginExtended.AddVector(extendedVector, noteDocId);
                    _ginReduced.AddVector(reducedVector, noteDocId);
                }

                var noteDbId = new DocId(note.NoteId);
                if (!_generalDirectIndex.TryAdd(noteDbId, tokenLine))
                {
                    throw new RsseTokenizerException($"[{nameof(SearchEngineTokenizer)}] vector initialization error");
                }
            }
        }
        catch (Exception ex)
        {
            throw new RsseTokenizerException($"[{nameof(SearchEngineTokenizer)}] initialization system error | " +
                                             $"'{ex.Source}' | '{ex.Message}'");
        }

        var count = _generalDirectIndex.Count;

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
    public Dictionary<DocId, double> ComputeComplianceIndices(string text, CancellationToken cancellationToken)
    {
        var metricsCalculator = new MetricsCalculator();

        var continueSearching = _searchProcessorFactory
            .ExtendedSearchProcessor
            .FindExtended(text, metricsCalculator, cancellationToken);

        if (!metricsCalculator.ContinueSearching || !continueSearching)
        {
            return metricsCalculator.ComplianceMetrics;
        }

        _searchProcessorFactory
            .ReducedSearchProcessor
            .FindReduced(text, metricsCalculator, cancellationToken);

        return metricsCalculator.ComplianceMetrics;
    }

    /// <summary>
    /// Создать два вектора токенов для заметки.
    /// </summary>
    /// <param name="factory">Фабрика токенайзеров.</param>
    /// <param name="note">Текстовая нагрузка заметки.</param>
    /// <returns>Векторы на базе двух разных эталонных наборов.</returns>
    private static TokenLine CreateTokensLine(ITokenizerProcessorFactory factory, TextRequestDto note)
    {
        var text = note.Text + ' ' + note.Title;

        // расширенная эталонная последовательность:
        var extendedProcessor = factory.CreateProcessor(ProcessorType.Extended);

        var extendedTokenLine = extendedProcessor.TokenizeText(text);

        // урезанная эталонная последовательность:
        var reducedProcessor = factory.CreateProcessor(ProcessorType.Reduced);

        var reducedTokenLine = reducedProcessor.TokenizeText(text);

        return new TokenLine(Extended: extendedTokenLine, Reduced: reducedTokenLine);
    }

    public void Dispose()
    {
        TokenizerLock.Dispose();
    }
}
