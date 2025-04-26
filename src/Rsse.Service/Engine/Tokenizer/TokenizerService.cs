using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SearchEngine.Common.Configuration;
using SearchEngine.Data.Context;
using SearchEngine.Data.Entities;
using SearchEngine.Data.Repository.Contracts;
using SearchEngine.Engine.Contracts;

namespace SearchEngine.Engine.Tokenizer;

/// <summary>
/// Сервис поддержки токенайзера
/// </summary>
public class TokenizerService : ITokenizerService
{
    // TODO использовать либо ReaderWriterLockSlim, либо ConcurrentDictionary
    // TODO дополнить логирование ошибок пересозданием линии кэша
    private readonly IServiceScopeFactory _factory;
    private readonly ConcurrentDictionary<int, List<int>> _reducedTokenLines;
    private readonly ConcurrentDictionary<int, List<int>> _extendedTokenLines;
    private readonly ReaderWriterLockSlim _lockSlim;
    private readonly ILogger<TokenizerService> _logger;
    private readonly bool _isEnabled;

    /// <summary>
    /// Создать и инициализировать сервис токенайзера
    /// </summary>
    /// <param name="factory">DI-фабрика</param>
    /// <param name="options">настройки</param>
    /// <param name="logger">логер</param>
    public TokenizerService(IServiceScopeFactory factory, IOptions<CommonBaseOptions> options, ILogger<TokenizerService> logger)
    {
        _factory = factory;
        _reducedTokenLines = new ConcurrentDictionary<int, List<int>>();
        _extendedTokenLines = new ConcurrentDictionary<int, List<int>>();
        _logger = logger;
        _lockSlim = new ReaderWriterLockSlim();
        _isEnabled = options.Value.TokenizerIsEnable;

        DatabaseInitializer.CreateAndSeed(_factory, _logger);
        Initialize();
    }

    /// <inheritdoc/>
    public ConcurrentDictionary<int, List<int>> GetReducedLines() => _reducedTokenLines;

    /// <inheritdoc/>
    public ConcurrentDictionary<int, List<int>> GetExtendedLines() => _extendedTokenLines;

    /// <inheritdoc/>
    public void Delete(int id)
    {
        if (!_isEnabled) return;

        _lockSlim.EnterWriteLock();

        var isReducedRemoved = _reducedTokenLines.TryRemove(id, out _);

        var isExtendedRemoved = _extendedTokenLines.TryRemove(id, out _);

        if (!(isReducedRemoved && isExtendedRemoved))
        {
            _logger.LogError($"[{nameof(TokenizerService)}] delete error");
        }

        _lockSlim.ExitWriteLock();
    }

    /// <inheritdoc/>
    public void Create(int id, NoteEntity note)
    {
        if (!_isEnabled) return;

        _lockSlim.EnterReadLock();

        using var scope = _factory.CreateScope();

        var processor = scope.ServiceProvider.GetRequiredService<ITokenizerProcessor>();

        var (extendedLine, reducedLine, _) = CreateTokensLine(processor, note);

        if (!_extendedTokenLines.TryAdd(id, extendedLine))
        {
            _logger.LogError($"[{nameof(TokenizerService)}] extended vectors create error");
        }

        if (!_reducedTokenLines.TryAdd(id, reducedLine))
        {
            _logger.LogError($"[{nameof(TokenizerService)}] reduced vectors create error");
        }

        _lockSlim.ExitReadLock();
    }

    /// <inheritdoc/>
    public void Update(int id, NoteEntity note)
    {
        if (!_isEnabled) return;

        _lockSlim.EnterReadLock();

        using var scope = _factory.CreateScope();

        var processor = scope.ServiceProvider.GetRequiredService<ITokenizerProcessor>();

        var (extendedLine, reducedLine, _) = CreateTokensLine(processor, note);

        if (_extendedTokenLines.TryGetValue(id, out var cachedTokensLine))
        {
            if (!_extendedTokenLines.TryUpdate(id, extendedLine, cachedTokensLine))
            {
                _logger.LogError($"[{nameof(TokenizerService)}] extended vectors concurrent update error");
            }
        }
        else
        {
            _logger.LogError($"[{nameof(TokenizerService)}] extended vectors has not been updated");
        }

        if (_reducedTokenLines.TryGetValue(id, out cachedTokensLine))
        {
            if (!_reducedTokenLines.TryUpdate(id, reducedLine, cachedTokensLine))
            {
                _logger.LogError($"[{nameof(TokenizerService)}] reduced vectors concurrent update error");
            }
        }
        else
        {
            _logger.LogError($"[{nameof(TokenizerService)}] reduced vectors has not been updated");
        }

        _lockSlim.ExitReadLock();
    }

    /// <inheritdoc/>
    public void Initialize()
    {
        if (!_isEnabled) return;

        _lockSlim.EnterWriteLock();

        using var scope = _factory.CreateScope();

        using var repo = scope.ServiceProvider.GetRequiredService<IDataRepository>();

        var processor = scope.ServiceProvider.GetRequiredService<ITokenizerProcessor>();

        try
        {
            _reducedTokenLines.Clear();

            _extendedTokenLines.Clear();

            // TODO избавиться от загрузки всех записей из таблицы:
            var texts = repo.ReadAllNotes();

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
            _logger.LogError(ex, $"[{nameof(TokenizerService)}] initialization system error");
        }
        finally
        {
            _lockSlim.ExitWriteLock();
        }
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
}
