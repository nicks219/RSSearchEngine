using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SearchEngine.Configuration;
using SearchEngine.Data;
using SearchEngine.Data.Repository.Contracts;
using SearchEngine.Infrastructure.Tokenizer.Contracts;

namespace SearchEngine.Infrastructure.Tokenizer;

public class TokenizerService : ITokenizerService
{
    // [TODO]: нужен ли ConcurrentDictionary при ReaderWriterLockSlim?
    // [TODO]: можно заменить логгирование на пересоздание линии кэша
    private readonly IServiceScopeFactory _factory;
    private readonly ConcurrentDictionary<int, List<int>> _reducedTokenLines;
    private readonly ConcurrentDictionary<int, List<int>> _extendedTokenLines;
    private readonly ReaderWriterLockSlim _lockSlim;
    private readonly ILogger<TokenizerService> _logger;
    private readonly bool _isEnabled;

    public TokenizerService(IServiceScopeFactory factory, IOptions<CommonBaseOptions> options, ILogger<TokenizerService> logger)
    {
        _factory = factory;
        _reducedTokenLines = new ConcurrentDictionary<int, List<int>>();
        _extendedTokenLines = new ConcurrentDictionary<int, List<int>>();
        _logger = logger;
        _lockSlim = new ReaderWriterLockSlim();
        _isEnabled = options.Value.TokenizerIsEnable;

        Initialize();
    }

    public ConcurrentDictionary<int, List<int>> GetReducedLines() => _reducedTokenLines;

    public ConcurrentDictionary<int, List<int>> GetExtendedLines() => _extendedTokenLines;

    public void Delete(int id)
    {
        if (!_isEnabled) return;

        _lockSlim.EnterWriteLock();

        var isReducedRemoved = _reducedTokenLines.TryRemove(id, out _);

        var isExtendedRemoved = _extendedTokenLines.TryRemove(id, out _);

        if (!(isReducedRemoved && isExtendedRemoved))
        {
            _logger.LogError("[Cache Repository: concurrent delete error]");
        }

        _lockSlim.ExitWriteLock();
    }

    public void Create(int id, TextEntity text)
    {
        if (!_isEnabled) return;

        _lockSlim.EnterReadLock();

        using var scope = _factory.CreateScope();

        var processor = scope.ServiceProvider.GetRequiredService<ITokenizerProcessor>();

        var (extendedLine, reducedLine, _) = CreateTokensLine(processor, text);

        if (!_extendedTokenLines.TryAdd(id, extendedLine))
        {
            _logger.LogError("[Cache Repository: concurrent create error - 2]");
        }

        if (!_reducedTokenLines.TryAdd(id, reducedLine))
        {
            _logger.LogError("[Cache Repository: concurrent create error - 1]");
        }

        _lockSlim.ExitReadLock();
    }

    public void Update(int id, TextEntity text)
    {
        if (!_isEnabled) return;

        _lockSlim.EnterReadLock();

        using var scope = _factory.CreateScope();

        var processor = scope.ServiceProvider.GetRequiredService<ITokenizerProcessor>();

        var (extendedLine, reducedLine, _) = CreateTokensLine(processor, text);

        if (_extendedTokenLines.TryGetValue(id, out var cachedTokensLine))
        {
            if (!_extendedTokenLines.TryUpdate(id, extendedLine, cachedTokensLine))
            {
                _logger.LogError("[Cache Repository: concurrent update error - 1_2]");
            }
        }
        else
        {
            _logger.LogError("[Cache Repository: concurrent update error - 1_1]");
        }

        if (_reducedTokenLines.TryGetValue(id, out cachedTokensLine))
        {
            if (!_reducedTokenLines.TryUpdate(id, reducedLine, cachedTokensLine))
            {
                _logger.LogError("[Cache Repository: concurrent update error - 2_2]");
            }
        }
        else
        {
            _logger.LogError("[Cache Repository: concurrent update error - 2_1]");
        }

        _lockSlim.ExitReadLock();
    }

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

            // TODO: избавься от загрузки всех записей из таблицы:
            var texts = repo.ReadAllNotes();

            foreach (var text in texts)
            {
                var (extendedLine, reducedLine, id) = CreateTokensLine(processor, text);

                if (!_extendedTokenLines.TryAdd(id, extendedLine))
                {
                    throw new MethodAccessException("[Cache Repository Init: extended failed]");
                }

                if (!_reducedTokenLines.TryAdd(id, reducedLine))
                {
                    throw new MethodAccessException("[Cache Repository Init: reduced failed]");
                }
            }

            GC.Collect();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Cache Repository Init: general failed]");
        }
        finally
        {
            _lockSlim.ExitWriteLock();
        }
    }

    private static (List<int> Extended, List<int> Reduced, int Id) CreateTokensLine(ITokenizerProcessor processor, TextEntity text)
    {
        // extended tokens chain line:
        processor.SetupChain(ConsonantChain.Extended);

        var note = processor.PreProcessNote(text.Song + ' ' + text.Title);

        var extendedTokensLine = processor.TokenizeSequence(note);

        // reduced tokens chain line:
        processor.SetupChain(ConsonantChain.Reduced);

        note = processor.PreProcessNote(text.Song + ' ' + text.Title);

        var reducedTokensLine = processor.TokenizeSequence(note);

        return (Extended: extendedTokensLine, Reduced: reducedTokensLine, Id: text.TextId);
    }
}
