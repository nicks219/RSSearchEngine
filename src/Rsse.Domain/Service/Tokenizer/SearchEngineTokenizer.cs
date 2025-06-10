using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SearchEngine.Data.Contracts;
using SearchEngine.Data.Dto;
using SearchEngine.Data.Entities;
using SearchEngine.Exceptions;
using SearchEngine.Service.Contracts;
using SearchEngine.Service.Mapping;
using SearchEngine.Service.Tokenizer.Factory;
using SearchEngine.Service.Tokenizer.Processor;
using SearchEngine.Service.Tokenizer.Wrapper;

namespace SearchEngine.Service.Tokenizer;

/// <summary>
/// Функционал поддержки токенайзера.
/// </summary>
public sealed class SearchEngineTokenizer : ISearchEngineTokenizer
{
    private TokenizerLock TokenizerLock { get; } = new();
    private readonly ConcurrentDictionary<DocId, TokenLine> _tokenLines;
    private readonly GinHandler _extendedGin;
    private readonly GinHandler _reducedGin;
    private readonly MetricsProcessorFactory _metricsProcessorFactory;
    private readonly SearchType _searchType;

    private readonly ITokenizerProcessorFactory _processorFactory;

    /// <summary>
    /// Флаг инициалицации токенайзера.
    /// </summary>
    private volatile bool _isActivated;

    /// <summary>
    /// Создать токенайзер.
    /// </summary>
    /// <param name="processorFactory">Фабрика токенайзеров.</param>
    /// <param name="searchType">Тип оптимизации алгоритма поиска.</param>
    public SearchEngineTokenizer(ITokenizerProcessorFactory processorFactory, SearchType searchType = SearchType.Original)
    {
        _tokenLines = new ConcurrentDictionary<DocId, TokenLine>();
        _processorFactory = processorFactory;
        _searchType = searchType;

        _extendedGin = new GinHandler();
        _reducedGin = new GinHandler();
        _metricsProcessorFactory = new MetricsProcessorFactory(_extendedGin, _reducedGin, _tokenLines, _processorFactory, _searchType);
    }

    // Используется для тестов.
    internal ConcurrentDictionary<DocId, TokenLine> GetTokenLines() => _tokenLines;

    /// <inheritdoc/>
    public async Task<bool> DeleteAsync(int id, CancellationToken stoppingToken)
    {
        using var __ = await TokenizerLock.AcquireExclusiveLockAsync(stoppingToken);
        var docId = new DocId(id);
        var removed = _tokenLines.TryRemove(docId, out _);

        return removed;
    }

    /// <inheritdoc/>
    public async Task<bool> CreateAsync(int id, TextRequestDto note, CancellationToken stoppingToken)
    {
        using var _ = await TokenizerLock.AcquireExclusiveLockAsync(stoppingToken);

        var createdTokenLine = CreateTokensLine(_processorFactory, note);

        var docId = new DocId(id);
        var created = _tokenLines.TryAdd(docId, createdTokenLine);

        return created;
    }

    /// <inheritdoc/>
    public async Task<bool> UpdateAsync(int id, TextRequestDto note, CancellationToken stoppingToken)
    {
        using var _ = await TokenizerLock.AcquireExclusiveLockAsync(stoppingToken);

        var updatedTokenLine = CreateTokensLine(_processorFactory, note);

        var docId = new DocId(id);
        if (!_tokenLines.TryGetValue(docId, out var existedLine))
        {
            return false;
        }

        var updated = _tokenLines.TryUpdate(docId, updatedTokenLine, existedLine);
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
            _tokenLines.Clear();

            // todo: подумать, как избавиться от загрузки всех записей из таблицы
            var notes = dataProvider.GetDataAsync().WithCancellation(stoppingToken);

            // todo: на старте сервиса при отсутствии коннекта до баз данных перечисление спамит логами с исключениями
            await foreach (var note in notes)
            {
                if (stoppingToken.IsCancellationRequested) throw new OperationCanceledException(nameof(InitializeAsync));

                var requestNote = note.MapToDto();

                var tokenLine = CreateTokensLine(_processorFactory, requestNote);

                if (_searchType != SearchType.Original)
                {
                    var noteDocId = new DocId(note.NoteId);
                    var extendedVector = tokenLine.Extended;
                    var reducedVector = tokenLine.Reduced;
                    _extendedGin.AddVectorToGin(extendedVector, noteDocId);
                    _reducedGin.AddVectorToGin(reducedVector, noteDocId);
                }

                var noteDbId = new DocId(note.NoteId);
                if (!_tokenLines.TryAdd(noteDbId, tokenLine))
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

        var count = _tokenLines.Count;

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
        var complianceMetrics = new Dictionary<DocId, double>();

        var continueSearching = _metricsProcessorFactory
            .ExtendedMetricsProcessor
            .FindExtended(text, complianceMetrics, cancellationToken);

        if (!continueSearching)
        {
            return complianceMetrics;
        }

        _metricsProcessorFactory
            .ReducedMetricsProcessor
            .FindReduced(text, complianceMetrics, cancellationToken);

        return complianceMetrics;
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
