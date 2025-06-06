using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SearchEngine.Api.Mapping;
using SearchEngine.Data.Contracts;
using SearchEngine.Data.Dto;
using SearchEngine.Data.Entities;
using SearchEngine.Service.Configuration;
using SearchEngine.Service.Contracts;
using SearchEngine.Service.Tokenizer;
using SearchEngine.Service.Tokenizer.Processor;

namespace SearchEngine.Api.Services;

/// <summary>
/// Сервис поддержки токенайзера.
/// </summary>
public sealed class TokenizerService : ITokenizerService, IDisposable
{
    private TokenizerLock TokenizerLock { get; } = new();
    private readonly ConcurrentDictionary<int, TokenLine> _tokenLines;

    private readonly ITokenizerProcessorFactory _processorFactory;
    private readonly ILogger<TokenizerService> _logger;
    private readonly bool _isEnabled;

    /// <summary>
    /// Флаг инициалицации токенайзера.
    /// </summary>
    private volatile bool _isActivated;

    /// <summary>
    /// Создать и инициализировать сервис токенайзера, вызывается раз в N часов.
    /// </summary>
    /// <param name="processorFactory">Фабрика токенайзеров.</param>
    /// <param name="options">Настройки.</param>
    /// <param name="logger">Логер.</param>
    public TokenizerService(
        ITokenizerProcessorFactory processorFactory,
        IOptions<CommonBaseOptions> options,
        ILogger<TokenizerService> logger)
    {
        _tokenLines = new ConcurrentDictionary<int, TokenLine>();
        _processorFactory = processorFactory;
        _logger = logger;
        _isEnabled = options.Value.TokenizerIsEnable;
    }

    // используется для тестов
    internal ConcurrentDictionary<int, TokenLine> GetTokenLines() => _tokenLines;

    /// <inheritdoc/>
    public async Task Delete(int id, CancellationToken stoppingToken)
    {
        if (!_isEnabled) return;

        using var __ = await TokenizerLock.AcquireExclusiveLockAsync(stoppingToken);
        var isRemoved = _tokenLines.TryRemove(id, out _);

        if (!isRemoved)
        {
            _logger.LogError($"[{nameof(TokenizerService)}] delete error");
        }
    }

    /// <inheritdoc/>
    public async Task Create(int id, TextRequestDto note, CancellationToken stoppingToken)
    {
        if (!_isEnabled) return;

        using var _ = await TokenizerLock.AcquireExclusiveLockAsync(stoppingToken);

        var createdTokenLine = CreateTokensLine(_processorFactory, note);

        if (!_tokenLines.TryAdd(id, createdTokenLine))
        {
            _logger.LogError($"[{nameof(TokenizerService)}] vectors create error");
        }
    }

    /// <inheritdoc/>
    public async Task Update(int id, TextRequestDto note, CancellationToken stoppingToken)
    {
        if (!_isEnabled) return;

        using var _ = await TokenizerLock.AcquireExclusiveLockAsync(stoppingToken);

        var updatedTokenLine = CreateTokensLine(_processorFactory, note);

        if (_tokenLines.TryGetValue(id, out var existedLine))
        {
            if (!_tokenLines.TryUpdate(id, updatedTokenLine, existedLine))
            {
                _logger.LogError($"[{nameof(TokenizerService)}] vectors concurrent update error");
            }
        }
        else
        {
            _logger.LogError($"[{nameof(TokenizerService)}] vectors has not been updated");
        }
    }

    /// <inheritdoc/>
    public async Task Initialize(IDataProvider<NoteEntity> dataProvider, CancellationToken stoppingToken)
    {
        if (!_isEnabled) return;

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
                if (stoppingToken.IsCancellationRequested) throw new OperationCanceledException(nameof(Initialize));

                var requestNote = note.MapToDto();

                var newTokenLine = CreateTokensLine(_processorFactory, requestNote);

                if (!_tokenLines.TryAdd(note.NoteId, newTokenLine))
                {
                    throw new MethodAccessException($"[{nameof(TokenizerService)}] vectors initialization error");
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError("[{Reporter}] initialization system error | '{Source}' | '{Message}'",
                nameof(TokenizerService), ex.Source, ex.Message);
        }

        _logger.LogInformation("[{Reporter}] initialization finished | data amount '{TokenLinesCount}'",
            nameof(TokenizerService), _tokenLines.Count);

        _isActivated = true;
    }

    /// <inheritdoc/>
    public async Task<bool> WaitWarmUp(CancellationToken timeoutToken)
    {
        if (_isEnabled == false) return true;

        await TokenizerLock.SyncOnLockAsync(timeoutToken);

        return _isActivated;
    }

    /// <inheritdoc/>
    public bool IsInitialized() => _isActivated;

    /// <inheritdoc/>
    // Сценарий: основная нагрузка приходится на операции чтения, в большинстве случаев со своими данными клиент работает единолично.
    // Допустимо, если метод вернёт неактуальные данные.
    public Dictionary<int, double> ComputeComplianceIndices(string text, CancellationToken cancellationToken)
    {
        var result = new Dictionary<int, double>();

        // I. коэффициент extended поиска: 0.8D
        const double extended = 0.8D;
        // II. коэффициент reduced поиска: 0.4D
        const double reduced = 0.6D; // 0.6 .. 0.75

        var reducedChainSearch = true;

        var processor = _processorFactory.CreateProcessor(ProcessorType.Extended);

        var preprocessedStrings = processor.PreProcessNote(text);

        if (preprocessedStrings.Count == 0)
        {
            // заметки вида "123 456" не ищем, так как получим весь каталог
            return result;
        }

        var newTokensLine = processor.TokenizeSequence(preprocessedStrings);

        // поиск в векторе extended
        if (cancellationToken.IsCancellationRequested) throw new OperationCanceledException(nameof(ComputeComplianceIndices));
        foreach (var (key, tokensLine) in _tokenLines)
        {
            var extendedTokensLine = tokensLine.Extended;
            var metric = processor.ComputeComparisionMetric(extendedTokensLine, newTokensLine);

            // I. 100% совпадение по extended последовательности, по reduced можно не искать
            if (metric == newTokensLine.Count)
            {
                reducedChainSearch = false;
                result.Add(key, metric * (1000D / extendedTokensLine.Count));
                continue;
            }

            // II. extended% совпадение
            if (metric >= newTokensLine.Count * extended)
            {
                // todo: можно так оценить
                // reducedChainSearch = false;
                result.Add(key, metric * (100D / extendedTokensLine.Count));
            }
        }

        if (!reducedChainSearch)
        {
            return result;
        }

        processor = _processorFactory.CreateProcessor(ProcessorType.Reduced);

        preprocessedStrings = processor.PreProcessNote(text);

        newTokensLine = processor.TokenizeSequence(preprocessedStrings);

        if (preprocessedStrings.Count == 0)
        {
            // песни вида "123 456" не ищем, так как получим весь каталог
            return result;
        }

        // убираем дубликаты слов для intersect - это меняет результаты поиска (тексты типа "казино казино казино")
        newTokensLine = newTokensLine.ToHashSet().ToList();

        // поиск в векторе reduced
        if (cancellationToken.IsCancellationRequested) throw new OperationCanceledException(nameof(ComputeComplianceIndices));
        foreach (var (key, tokensLine) in _tokenLines)
        {
            var reducedTokensLine = tokensLine.Reduced;
            var metric = processor.ComputeComparisionMetric(reducedTokensLine, newTokensLine);

            // III. 100% совпадение по reduced
            if (metric == newTokensLine.Count)
            {
                result.TryAdd(key, metric * (10D / reducedTokensLine.Count));
                continue;
            }

            // IV. reduced% совпадение - мы не можем наверняка оценить неточное совпадение
            if (metric >= newTokensLine.Count * reduced)
            {
                result.TryAdd(key, metric * (1D / reducedTokensLine.Count));
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
        // расширенная эталонная последовательность:
        var processor = factory.CreateProcessor(ProcessorType.Extended);

        var preprocessedNote = processor.PreProcessNote(note.Text + ' ' + note.Title);

        var extendedTokensLine = processor.TokenizeSequence(preprocessedNote);

        // урезанная эталонная последовательность:
        processor = factory.CreateProcessor(ProcessorType.Reduced);

        preprocessedNote = processor.PreProcessNote(note.Text + ' ' + note.Title);

        var reducedTokensLine = processor.TokenizeSequence(preprocessedNote);

        return new TokenLine(Extended: extendedTokensLine, Reduced: reducedTokensLine);
    }

    public void Dispose()
    {
        TokenizerLock.Dispose();
    }
}
