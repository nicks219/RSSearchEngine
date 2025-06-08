using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SearchEngine.Data.Contracts;
using SearchEngine.Data.Dto;
using SearchEngine.Data.Entities;
using SearchEngine.Exceptions;
using SearchEngine.Service.Contracts;
using SearchEngine.Service.Mapping;
using SearchEngine.Service.Tokenizer.Processor;

namespace SearchEngine.Service.Tokenizer;

/// <summary>
/// Функционал поддержки токенайзера.
/// </summary>
public sealed class SearchEngineTokenizer : ISearchEngineTokenizer
{
    private TokenizerLock TokenizerLock { get; } = new();
    private readonly ConcurrentDictionary<int, TokenLine> _tokenLines;

    private readonly ITokenizerProcessorFactory _processorFactory;

    /// <summary>
    /// Флаг инициалицации токенайзера.
    /// </summary>
    private volatile bool _isActivated;

    /// <summary>
    /// Создать токенайзер.
    /// </summary>
    /// <param name="processorFactory">Фабрика токенайзеров.</param>
    public SearchEngineTokenizer(ITokenizerProcessorFactory processorFactory)
    {
        _tokenLines = new ConcurrentDictionary<int, TokenLine>();
        _processorFactory = processorFactory;
    }

    // Используется для тестов.
    internal ConcurrentDictionary<int, TokenLine> GetTokenLines() => _tokenLines;

    /// <inheritdoc/>
    public async Task<bool> DeleteAsync(int id, CancellationToken stoppingToken)
    {
        using var __ = await TokenizerLock.AcquireExclusiveLockAsync(stoppingToken);
        var removed = _tokenLines.TryRemove(id, out _);

        return removed;
    }

    /// <inheritdoc/>
    public async Task<bool> CreateAsync(int id, TextRequestDto note, CancellationToken stoppingToken)
    {
        using var _ = await TokenizerLock.AcquireExclusiveLockAsync(stoppingToken);

        var createdTokenLine = CreateTokensLine(_processorFactory, note);

        var created = _tokenLines.TryAdd(id, createdTokenLine);

        return created;
    }

    /// <inheritdoc/>
    public async Task<bool> UpdateAsync(int id, TextRequestDto note, CancellationToken stoppingToken)
    {
        using var _ = await TokenizerLock.AcquireExclusiveLockAsync(stoppingToken);

        var updatedTokenLine = CreateTokensLine(_processorFactory, note);

        if (!_tokenLines.TryGetValue(id, out var existedLine))
        {
            return false;
        }

        var updated = _tokenLines.TryUpdate(id, updatedTokenLine, existedLine);
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

                var newTokenLine = CreateTokensLine(_processorFactory, requestNote);

                if (!_tokenLines.TryAdd(note.NoteId, newTokenLine))
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
    public Dictionary<int, double> ComputeComplianceIndices(string text, CancellationToken cancellationToken)
    {
        var result = new Dictionary<int, double>();

        // I. коэффициент extended поиска: 0.8D
        const double extended = 0.8D;
        // II. коэффициент reduced поиска: 0.4D
        const double reduced = 0.6D; // 0.6 .. 0.75

        var reducedChainSearch = true;

        var processor = _processorFactory.CreateProcessor(ProcessorType.Extended);

        var extendedTokenizedText = processor.TokenizeText(text);

        if (extendedTokenizedText.Count == 0)
        {
            // заметки вида "123 456" не ищем, так как получим весь каталог
            return result;
        }

        // поиск в векторе extended
        if (cancellationToken.IsCancellationRequested) throw new OperationCanceledException(nameof(ComputeComplianceIndices));
        foreach (var (id, tokenLine) in _tokenLines)
        {
            var extendedTokenLine = tokenLine.Extended;
            var metric = processor.ComputeComparisionMetric(extendedTokenLine, extendedTokenizedText);

            // I. 100% совпадение по extended последовательности, по reduced можно не искать
            if (metric == extendedTokenizedText.Count)
            {
                reducedChainSearch = false;
                result.Add(id, metric * (1000D / extendedTokenLine.Count));
                continue;
            }

            // II. extended% совпадение
            if (metric >= extendedTokenizedText.Count * extended)
            {
                // todo: можно так оценить
                // reducedChainSearch = false;
                result.Add(id, metric * (100D / extendedTokenLine.Count));
            }
        }

        if (!reducedChainSearch)
        {
            return result;
        }

        processor = _processorFactory.CreateProcessor(ProcessorType.Reduced);

        var reducedTokenizedText = processor.TokenizeText(text);

        if (reducedTokenizedText.Count == 0)
        {
            // песни вида "123 456" не ищем, так как получим весь каталог
            return result;
        }

        // убираем дубликаты слов для intersect - это меняет результаты поиска (тексты типа "казино казино казино")
        reducedTokenizedText = reducedTokenizedText.ToHashSet().ToList();

        // поиск в векторе reduced
        if (cancellationToken.IsCancellationRequested) throw new OperationCanceledException(nameof(ComputeComplianceIndices));
        foreach (var (id, tokenLine) in _tokenLines)
        {
            var reducedTokenLine = tokenLine.Reduced;
            var metric = processor.ComputeComparisionMetric(reducedTokenLine, reducedTokenizedText);

            // III. 100% совпадение по reduced
            if (metric == reducedTokenizedText.Count)
            {
                result.TryAdd(id, metric * (10D / reducedTokenLine.Count));
                continue;
            }

            // IV. reduced% совпадение - мы не можем наверняка оценить неточное совпадение
            if (metric >= reducedTokenizedText.Count * reduced)
            {
                result.TryAdd(id, metric * (1D / reducedTokenLine.Count));
            }
        }

        return result;
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
