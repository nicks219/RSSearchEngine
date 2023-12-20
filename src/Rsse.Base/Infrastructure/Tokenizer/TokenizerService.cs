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
using SearchEngine.Infrastructure.Engine.Contracts;
using SearchEngine.Infrastructure.Tokenizer.Contracts;

namespace SearchEngine.Infrastructure.Tokenizer;

public class TokenizerService : ITokenizerService
{
    // [TODO]: нужен ли ConcurrentDictionary при ReaderWriterLockSlim?
    // [TODO]: можно заменить логгирование на пересоздание линии кэша
    private readonly IServiceScopeFactory _factory;
    private readonly ConcurrentDictionary<int, List<int>> _undefinedTokenLines;
    private readonly ConcurrentDictionary<int, List<int>> _definedTokenLines;
    private readonly ReaderWriterLockSlim _lockSlim;
    private readonly ILogger<TokenizerService> _logger;
    private readonly bool _isEnabled;

    public TokenizerService(IServiceScopeFactory factory, IOptions<CommonBaseOptions> options, ILogger<TokenizerService> logger)
    {
        _factory = factory;
        _undefinedTokenLines = new ConcurrentDictionary<int, List<int>>();
        _definedTokenLines = new ConcurrentDictionary<int, List<int>>();
        _logger = logger;
        _lockSlim = new ReaderWriterLockSlim();
        _isEnabled = options.Value.TokenizerIsEnable;

        Initialize();
    }

    public ConcurrentDictionary<int, List<int>> GetUndefinedLines()
    {
        return _undefinedTokenLines;
    }

    public ConcurrentDictionary<int, List<int>> GetDefinedLines()
    {
        return _definedTokenLines;
    }

    public void Delete(int id)
    {
        if (!_isEnabled) return;

        _lockSlim.EnterWriteLock();

        var res1 = _undefinedTokenLines.TryRemove(id, out _);

        var res2 = _definedTokenLines.TryRemove(id, out _);

        if (!(res1 && res2))
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

        var (definedHash, undefinedHash, _) = CreateCacheLine(processor, text);

        if (!_undefinedTokenLines.TryAdd(id, undefinedHash))
        {
            _logger.LogError("[Cache Repository: concurrent create error - 1]");
        }

        if (!_definedTokenLines.TryAdd(id, definedHash))
        {
            _logger.LogError("[Cache Repository: concurrent create error - 2]");
        }

        _lockSlim.ExitReadLock();
    }

    public void Update(int id, TextEntity text)
    {
        if (!_isEnabled) return;

        _lockSlim.EnterReadLock();

        using var scope = _factory.CreateScope();

        var processor = scope.ServiceProvider.GetRequiredService<ITokenizerProcessor>();

        var (definedHash, undefinedHash, _) = CreateCacheLine(processor, text);

        if (_undefinedTokenLines.TryGetValue(id, out var oldHash))
        {
            if (!_undefinedTokenLines.TryUpdate(id, undefinedHash, oldHash))
            {
                _logger.LogError("[Cache Repository: concurrent update error - 1_2]");
            }
        }
        else
        {
            _logger.LogError("[Cache Repository: concurrent update error - 1_1]");
        }

        if (_definedTokenLines.TryGetValue(id, out oldHash))
        {
            if (!_definedTokenLines.TryUpdate(id, definedHash, oldHash))
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
            _undefinedTokenLines.Clear();

            _definedTokenLines.Clear();

            // TODO: избавься от загрузки всех записей из таблицы:
            var texts = repo.ReadAllNotes();

            foreach (var text in texts)
            {
                var (definedHash, undefinedHash, songNumber) = CreateCacheLine(processor, text);

                if (!_undefinedTokenLines.TryAdd(songNumber, undefinedHash))
                {
                    throw new MethodAccessException("[Cache Repository Init: undefined failed]");
                }

                if (!_definedTokenLines.TryAdd(songNumber, definedHash))
                {
                    throw new MethodAccessException("[Cache Repository Init: defined failed]");
                }
            }

            GC.Collect();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Cache Repository Init: Init Failed]");
        }
        finally
        {
            _lockSlim.ExitWriteLock();
        }
    }

    private static (List<int> Def, List<int> Undef, int Num) CreateCacheLine(ITokenizerProcessor processor, TextEntity text)
    {
        // undefined hash line
        processor.Setup(ConsonantChain.Undefined);

        //var song = processor.ConvertStringToText(text);

        //song.Title.ForEach(t => song.Words.Add(t));

        var song = processor.CleanUpString(text.Song + ' ' + text.Title);

        var undefinedHashLine = processor.GetHashSetFromStrings(song);

        // defined hash line
        processor.Setup(ConsonantChain.Defined);

        //song = processor.ConvertStringToText(text);

        //song.Title.ForEach(t => song.Words.Add(t));

        song = processor.CleanUpString(text.Song + ' ' + text.Title);

        var definedHashLine = processor.GetHashSetFromStrings(song);

        return (Def: definedHashLine, Undef: undefinedHashLine, Num: text.TextId);
    }
}
