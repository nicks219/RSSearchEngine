using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SearchEngine.Domain.Configuration;
using SearchEngine.Domain.Contracts;
using SearchEngine.Domain.Entities;

namespace SearchEngine.Domain.Tokenizer;

/// <summary>
/// Сервис поддержки токенайзера
/// </summary>
public class TokenizerService : ITokenizerService, IDisposable
{
    private TokenizerLock TokenizerLock { get; } = new();
    private readonly ConcurrentDictionary<int, TokenLine> _tokenLines;

    private readonly IServiceProvider _provider;
    private readonly ILogger<TokenizerService> _logger;
    private readonly bool _isEnabled;

    /// <summary>
    /// Создать и инициализировать сервис токенайзера, вызывается раз в N часов
    /// </summary>
    /// <param name="provider">DI-фабрика</param>
    /// <param name="options">настройки</param>
    /// <param name="logger">логер</param>
    public TokenizerService(IServiceProvider provider, IOptions<CommonBaseOptions> options, ILogger<TokenizerService> logger)
    {
        _provider = provider;
        _tokenLines = new ConcurrentDictionary<int, TokenLine>();
        _logger = logger;
        _isEnabled = options.Value.TokenizerIsEnable;
    }

    // используется для тестов
    internal ConcurrentDictionary<int, TokenLine> GetTokenLines() => _tokenLines;

    /// <inheritdoc/>
    public async Task Delete(int id)
    {
        if (!_isEnabled) return;

        using var __ = await TokenizerLock.AcquireExclusiveLockAsync();
        var isRemoved = _tokenLines.TryRemove(id, out _);

        if (!isRemoved)
        {
            _logger.LogError($"[{nameof(TokenizerService)}] delete error");
        }
    }

    /// <inheritdoc/>
    public async Task Create(int id, NoteEntity note)
    {
        if (!_isEnabled) return;

        using var _ = await TokenizerLock.AcquireExclusiveLockAsync();
        using var processor = _provider.GetRequiredService<ITokenizerProcessor>();

        var createdTokenLine = CreateTokensLine(processor, note);

        if (!_tokenLines.TryAdd(id, createdTokenLine))
        {
            _logger.LogError($"[{nameof(TokenizerService)}] vectors create error");
        }
    }

    /// <inheritdoc/>
    public async Task Update(int id, NoteEntity note)
    {
        if (!_isEnabled) return;

        using var _ = await TokenizerLock.AcquireExclusiveLockAsync();
        using var processor = _provider.GetRequiredService<ITokenizerProcessor>();

        var updatedTokenLine = CreateTokensLine(processor, note);

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
    public async Task Initialize()
    {
        if (!_isEnabled) return;

        // Инициализация вызывается не только не старте сервиса и её следует разграничить с остальными меняющими данные операций.
        using var _ = await TokenizerLock.AcquireExclusiveLockAsync();

        // Не закрываем контекст в корневом scope провайдера.
        await using var repo = _provider.CreateScope().ServiceProvider.GetRequiredService<IDataRepository>();

        using var processor = _provider.GetRequiredService<ITokenizerProcessor>();

        try
        {
            _tokenLines.Clear();

            // todo: избавиться от загрузки всех записей из таблицы:
            var notes = repo.ReadAllNotes();

            // todo: на старте сервиса при отсутствии коннекта до баз данных перечисление спамит логами с исключениями
            await foreach (var note in notes)
            {
                var newTokenLine = CreateTokensLine(processor, note);

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
    }

    /// <inheritdoc/>
    public async Task WaitWarmUp()
    {
        if (!_isEnabled) return;

        await TokenizerLock.SyncOnLockAsync();
    }

    /// <inheritdoc/>
    // Сценарий: основная нагрузка приходится на операции чтения, в большинстве случаев со своими данными клиент работает единолично.
    // Допустимо, если метод вернёт неактуальные данные.
    public Dictionary<int, double> ComputeComplianceIndices(string text)
    {
        using var processor = _provider.GetRequiredService<ITokenizerProcessor>();

        var result = new Dictionary<int, double>();

        // I. коэффициент extended поиска: 0.8D
        const double extended = 0.8D;
        // II. коэффициент reduced поиска: 0.4D
        const double reduced = 0.6D; // 0.6 .. 0.75

        var reducedChainSearch = true;

        processor.SetupChain(ConsonantChain.Extended);

        var preprocessedStrings = processor.PreProcessNote(text);

        if (preprocessedStrings.Count == 0)
        {
            // заметки вида "123 456" не ищем, так как получим весь каталог
            return result;
        }

        var newTokensLine = processor.TokenizeSequence(preprocessedStrings);

        // поиск в векторе extended
        foreach (var (key, tokensLine) in _tokenLines)
        {
            var extendedTokensLine = tokensLine.Extended;
            var metric = processor.ComputeComparisionMetric(extendedTokensLine, newTokensLine);

            // I. 100% совпадение по extended последовательности, по reduced можно не искать
            if (metric == newTokensLine.Count)
            {
                reducedChainSearch = false;
                result.Add(key, metric * (1000D / extendedTokensLine.Count)); // было int
                continue;
            }

            // II. extended% совпадение
            if (metric >= newTokensLine.Count * extended)
            {
                // [TODO] можно так оценить
                // reducedChainSearch = false;
                result.Add(key, metric * (100D / extendedTokensLine.Count)); // было int
            }
        }

        if (!reducedChainSearch)
        {
            return result;
        }

        processor.SetupChain(ConsonantChain.Reduced);

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
        foreach (var (key, tokensLine) in _tokenLines)
        {
            var reducedTokensLine = tokensLine.Reduced;
            var metric = processor.ComputeComparisionMetric(reducedTokensLine, newTokensLine);

            // III. 100% совпадение по reduced
            if (metric == newTokensLine.Count)
            {
                result.TryAdd(key, metric * (10D / reducedTokensLine.Count)); // было int
                continue;
            }

            // IV. reduced% совпадение - мы не можем наверняка оценить неточное совпадение
            if (metric >= newTokensLine.Count * reduced)
            {
                result.TryAdd(key, metric * (1D / reducedTokensLine.Count)); // было int
            }
        }

        return result;
    }

    /// <summary>
    /// Создать два вектора токенов для заметки
    /// </summary>
    /// <param name="processor">токенайзер</param>
    /// <param name="note">заметка</param>
    /// <returns>векторы на базе двух разных эталонных наборов</returns>
    private static TokenLine CreateTokensLine(ITokenizerProcessor processor, NoteEntity note)
    {
        // расширенная эталонная последовательность:
        processor.SetupChain(ConsonantChain.Extended);

        var preprocessedNote = processor.PreProcessNote(note.Text + ' ' + note.Title);

        var extendedTokensLine = processor.TokenizeSequence(preprocessedNote);

        // урезанная эталонная последовательность:
        processor.SetupChain(ConsonantChain.Reduced);

        preprocessedNote = processor.PreProcessNote(note.Text + ' ' + note.Title);

        var reducedTokensLine = processor.TokenizeSequence(preprocessedNote);

        return new TokenLine(Extended: extendedTokensLine, Reduced: reducedTokensLine);
    }

    public void Dispose()
    {
        TokenizerLock.Dispose();
    }
}
