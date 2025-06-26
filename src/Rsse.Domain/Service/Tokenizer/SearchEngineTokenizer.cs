using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Rsse.Search.Dto;
using Rsse.Search.Indexes;
using SearchEngine.Data.Contracts;
using SearchEngine.Data.Dto;
using SearchEngine.Data.Entities;
using SearchEngine.Exceptions;
using SearchEngine.Service.Mapping;
using SearchEngine.Service.Tokenizer.Contracts;
using SearchEngine.Service.Tokenizer.SearchProcessor;
using SearchEngine.Service.Tokenizer.TokenizerProcessor;

namespace SearchEngine.Service.Tokenizer;

/// <summary>
/// Функционал поддержки токенайзера.
/// </summary>
public sealed class SearchEngineTokenizer : ISearchEngineTokenizer
{
    private TokenizerLock TokenizerLock { get; } = new();
    private readonly SearchProcessorFactory _searchProcessorFactory = new();
    private readonly ExtendedSearchType _extendedSearchType;
    private readonly ReducedSearchType _reducedSearchType;

    /// <summary>
    /// Фабрика токенизаторов.
    /// </summary>
    private readonly ITokenizerProcessorFactory _tokenizerProcessorFactory;

    /// <summary>
    /// Флаг инициалицации токенайзера.
    /// </summary>
    private volatile bool _isActivated;

    /// <summary>
    /// Создать токенайзер.
    /// </summary>
    /// <param name="tokenizerProcessorFactory">Фабрика токенайзеров.</param>
    /// <param name="extendedSearchType">Тип оптимизации алгоритма поиска.</param>
    /// <param name="reducedSearchType">Тип оптимизации алгоритма поиска.</param>
    public SearchEngineTokenizer(
        ITokenizerProcessorFactory tokenizerProcessorFactory,
        ExtendedSearchType extendedSearchType = ExtendedSearchType.Original,
        ReducedSearchType reducedSearchType = ReducedSearchType.Original)
    {
        _tokenizerProcessorFactory = tokenizerProcessorFactory;
        _extendedSearchType = extendedSearchType;
        _reducedSearchType = reducedSearchType;
    }

    // Используется для тестов.
    internal DirectIndex GetTokenLines() => _searchProcessorFactory.GetTokenLines();

    /// <inheritdoc/>
    public async Task<bool> DeleteAsync(int id, CancellationToken stoppingToken)
    {
        using var __ = await TokenizerLock.AcquireExclusiveLockAsync(stoppingToken);

        var documentId = new DocumentId(id);

        var removed = _searchProcessorFactory.TryRemove(documentId);

        return removed;
    }

    /// <inheritdoc/>
    public async Task<bool> CreateAsync(int id, TextRequestDto note, CancellationToken stoppingToken)
    {
        using var _ = await TokenizerLock.AcquireExclusiveLockAsync(stoppingToken);

        var createdTokenLine = CreateTokensLine(note);
        var docId = new DocumentId(id);

        var created = _searchProcessorFactory.TryAdd(docId, createdTokenLine);

        return created;
    }

    /// <inheritdoc/>
    public async Task<bool> UpdateAsync(int id, TextRequestDto note, CancellationToken stoppingToken)
    {
        using var _ = await TokenizerLock.AcquireExclusiveLockAsync(stoppingToken);

        var updatedTokenLine = CreateTokensLine(note);
        var docId = new DocumentId(id);

        var updated = _searchProcessorFactory.TryUpdate(docId, updatedTokenLine);

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
            _searchProcessorFactory.Clear();

            // todo: подумать, как избавиться от загрузки всех записей из таблицы
            var notes = dataProvider.GetDataAsync().WithCancellation(stoppingToken);

            // todo: на старте сервиса при отсутствии коннекта до баз данных перечисление спамит логами с исключениями
            await foreach (var note in notes)
            {
                if (stoppingToken.IsCancellationRequested) throw new OperationCanceledException(nameof(InitializeAsync));

                var requestNote = note.MapToDto();
                var tokenLine = CreateTokensLine(requestNote);
                var noteDocId = new DocumentId(note.NoteId);

                if (!_searchProcessorFactory.TryAdd(noteDocId, tokenLine))
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

        var count = _searchProcessorFactory.Count;

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

        var extendedProcessor = _tokenizerProcessorFactory.CreateProcessor(ProcessorType.Extended);

        var extendedSearchVector = extendedProcessor.TokenizeText(text);

        if (extendedSearchVector.Count == 0)
        {
            // заметки вида "123 456" не ищем, так как получим весь каталог
            return metricsCalculator.ComplianceMetrics;
        }

        _searchProcessorFactory.FindExtended(_extendedSearchType,
            extendedSearchVector, metricsCalculator, cancellationToken);

        if (!metricsCalculator.ContinueSearching)
        {
            return metricsCalculator.ComplianceMetrics;
        }

        var reducedProcessor = _tokenizerProcessorFactory.CreateProcessor(ProcessorType.Reduced);

        TokenVector reducedSearchVector = reducedProcessor.TokenizeText(text);

        if (reducedSearchVector.Count == 0)
        {
            // песни вида "123 456" не ищем, так как получим весь каталог
            return metricsCalculator.ComplianceMetrics;
        }

        _searchProcessorFactory.FindReduced(_reducedSearchType,
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
        var extendedProcessor = _tokenizerProcessorFactory.CreateProcessor(ProcessorType.Extended);

        var extendedTokenLine = extendedProcessor.TokenizeText(note.Text, " ", note.Title);

        // урезанная эталонная последовательность:
        var reducedProcessor = _tokenizerProcessorFactory.CreateProcessor(ProcessorType.Reduced);

        var reducedTokenLine = reducedProcessor.TokenizeText(note.Text, " ", note.Title);

        return new TokenLine(Extended: extendedTokenLine, Reduced: reducedTokenLine);
    }

    public void Dispose()
    {
        TokenizerLock.Dispose();
    }
}
