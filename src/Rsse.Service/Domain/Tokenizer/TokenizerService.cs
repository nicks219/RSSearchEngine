using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
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

    private readonly ConcurrentDictionary<int, List<int>> _reducedTokenLines;
    private readonly ConcurrentDictionary<int, List<int>> _extendedTokenLines;

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
        _reducedTokenLines = new ConcurrentDictionary<int, List<int>>();
        _extendedTokenLines = new ConcurrentDictionary<int, List<int>>();
        _logger = logger;
        _isEnabled = options.Value.TokenizerIsEnable;
    }

    // используется для тестов
    internal ConcurrentDictionary<int, List<int>> GetReducedLines() => _reducedTokenLines;

    // используется для тестов
    internal ConcurrentDictionary<int, List<int>> GetExtendedLines() => _extendedTokenLines;

    /// <inheritdoc/>
    public void Delete(int id)
    {
        if (!_isEnabled) return;

        using var __ = TokenizerLock.AcquireSharedLock();

        var isReducedRemoved = _reducedTokenLines.TryRemove(id, out _);

        var isExtendedRemoved = _extendedTokenLines.TryRemove(id, out _);

        if (!(isReducedRemoved && isExtendedRemoved))
        {
            _logger.LogError($"[{nameof(TokenizerService)}] delete error");
        }
    }

    /// <inheritdoc/>
    public void Create(int id, NoteEntity note)
    {
        if (!_isEnabled) return;

        using var _ = TokenizerLock.AcquireSharedLock();

        using var processor = _provider.GetRequiredService<ITokenizerProcessor>();

        var (extendedLine, reducedLine, _) = CreateTokensLine(processor, note);

        if (!_extendedTokenLines.TryAdd(id, extendedLine))
        {
            _logger.LogError($"[{nameof(TokenizerService)}] extended vectors create error");
        }

        if (!_reducedTokenLines.TryAdd(id, reducedLine))
        {
            _logger.LogError($"[{nameof(TokenizerService)}] reduced vectors create error");
        }
    }

    /// <inheritdoc/>
    public void Update(int id, NoteEntity note)
    {
        if (!_isEnabled) return;

        using var _ = TokenizerLock.AcquireSharedLock();

        using var processor = _provider.GetRequiredService<ITokenizerProcessor>();

        var (extendedLine, reducedLine, _) = CreateTokensLine(processor, note);

        if (_extendedTokenLines.TryGetValue(id, out var existedLine))
        {
            if (!_extendedTokenLines.TryUpdate(id, extendedLine, existedLine))
            {
                _logger.LogError($"[{nameof(TokenizerService)}] extended vectors concurrent update error");
            }
        }
        else
        {
            _logger.LogError($"[{nameof(TokenizerService)}] extended vectors has not been updated");
        }

        if (_reducedTokenLines.TryGetValue(id, out existedLine))
        {
            if (!_reducedTokenLines.TryUpdate(id, reducedLine, existedLine))
            {
                _logger.LogError($"[{nameof(TokenizerService)}] reduced vectors concurrent update error");
            }
        }
        else
        {
            _logger.LogError($"[{nameof(TokenizerService)}] reduced vectors has not been updated");
        }
    }

    /// <inheritdoc/>
    public void Initialize()
    {
        if (!_isEnabled) return;

        // Инициализация вызывается не только не старте сервиса и её следует разграничить с остальными меняющими данные операций.
        using var _ = TokenizerLock.AcquireExclusiveLock();

        // Не закрываем контекст в корневом scope провайдера.
        using var repo = _provider.CreateScope().ServiceProvider.GetRequiredService<IDataRepository>();

        using var processor = _provider.GetRequiredService<ITokenizerProcessor>();

        try
        {
            _reducedTokenLines.Clear();

            _extendedTokenLines.Clear();

            // todo: избавиться от загрузки всех записей из таблицы:
            var texts = repo.ReadAllNotes();

            // todo: на старте сервиса при отсутствии коннекта до баз данных перечисление спамит логами с исключениями
            foreach (var text in texts)
            {
                var (extendedLine, reducedLine, id) = CreateTokensLine(processor, text);

                if (!_extendedTokenLines.TryAdd(id, extendedLine))
                {
                    throw new MethodAccessException($"[{nameof(TokenizerService)}] extended vectors initialization error");
                }

                if (!_reducedTokenLines.TryAdd(id, reducedLine))
                {
                    throw new MethodAccessException($"[{nameof(TokenizerService)}] reduced vectors initialization error");
                }
            }

            GC.Collect();
        }
        catch (Exception ex)
        {
            _logger.LogError("[{Reporter}] initialization system error | '{Source}' | '{Message}'", nameof(TokenizerService), ex.Source, ex.Message);
        }

        _logger.LogInformation("[{Reporter}] initialization finished | data amount '{Extended}'-'{Reduced}'", nameof(TokenizerService), _extendedTokenLines.Count, _reducedTokenLines.Count);
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

        foreach (var (key, cachedTokensLine) in _extendedTokenLines)
        {
            var metric = processor.ComputeComparisionMetric(cachedTokensLine, newTokensLine);

            // I. 100% совпадение по extended последовательности, по reduced можно не искать
            if (metric == newTokensLine.Count)
            {
                reducedChainSearch = false;
                result.Add(key, metric * (1000D / cachedTokensLine.Count)); // было int
                continue;
            }

            // II. extended% совпадение
            if (metric >= newTokensLine.Count * extended)
            {
                // [TODO] можно так оценить
                // reducedChainSearch = false;
                result.Add(key, metric * (100D / cachedTokensLine.Count)); // было int
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

        foreach (var (key, cachedTokensLine) in _reducedTokenLines)
        {
            var metric = processor.ComputeComparisionMetric(cachedTokensLine, newTokensLine);

            // III. 100% совпадение по reduced
            if (metric == newTokensLine.Count)
            {
                result.TryAdd(key, metric * (10D / cachedTokensLine.Count)); // было int
                continue;
            }

            // IV. reduced% совпадение - мы не можем наверняка оценить неточное совпадение
            if (metric >= newTokensLine.Count * reduced)
            {
                result.TryAdd(key, metric * (1D / cachedTokensLine.Count)); // было int
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
    private static (List<int> Extended, List<int> Reduced, int Id) CreateTokensLine(ITokenizerProcessor processor, NoteEntity note)
    {
        // расширенная эталонная последовательность:
        processor.SetupChain(ConsonantChain.Extended);

        var preprocessedNote = processor.PreProcessNote(note.Text + ' ' + note.Title);

        var extendedTokensLine = processor.TokenizeSequence(preprocessedNote);

        // урезанная эталонная последовательность:
        processor.SetupChain(ConsonantChain.Reduced);

        preprocessedNote = processor.PreProcessNote(note.Text + ' ' + note.Title);

        var reducedTokensLine = processor.TokenizeSequence(preprocessedNote);

        return (Extended: extendedTokensLine, Reduced: reducedTokensLine, Id: note.NoteId);
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        TokenizerLock.Dispose();
    }
}
