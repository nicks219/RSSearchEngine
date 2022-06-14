using System.Collections.Concurrent;
using RandomSongSearchEngine.Data.Repository.Contracts;
using RandomSongSearchEngine.Infrastructure.Cache.Contracts;
using RandomSongSearchEngine.Infrastructure.Engine;
using RandomSongSearchEngine.Infrastructure.Engine.Contracts;

namespace RandomSongSearchEngine.Infrastructure.Cache;

public class CacheRepository : ICacheRepository
{
    // [TODO]: нужен ли ConcurrentDictionary при ReaderWriterLockSlim?
    // [TODO]: можно заменить логгирование на пересоздание линии кэша
    private readonly IServiceScopeFactory _factory;
    private readonly ConcurrentDictionary<int, List<int>> _undefinedCache;
    private readonly ConcurrentDictionary<int, List<int>> _definedCache;
    private readonly ReaderWriterLockSlim _lockSlim;
    private readonly ILogger<CacheRepository> _logger;

    public CacheRepository(IServiceScopeFactory factory, ILogger<CacheRepository> logger)
    {
        _factory = factory;
        _undefinedCache = new ConcurrentDictionary<int, List<int>>();
        _definedCache = new ConcurrentDictionary<int, List<int>>();
        _logger = logger;
        _lockSlim = new ReaderWriterLockSlim();

        Initialize();
    }

    public ConcurrentDictionary<int, List<int>> GetUndefinedCache()
    {
        return _undefinedCache;
    }

    public ConcurrentDictionary<int, List<int>> GetDefinedCache()
    {
        return _definedCache;
    }

    public void Delete(int id)
    {
        _lockSlim.EnterWriteLock();

        var res1 = _undefinedCache.TryRemove(id, out _);

        var res2 = _definedCache.TryRemove(id, out _);

        if (!(res1 && res2))
        {
            _logger.LogError("[Cache Repository: concurrent delete error]");
        }

        _lockSlim.ExitWriteLock();
    }

    public void Create(int id, string text)
    {
        _lockSlim.EnterReadLock();

        using var scope = _factory.CreateScope();

        var processor = scope.ServiceProvider.GetRequiredService<ITextProcessor>();

        var (definedHash, undefinedHash, _) = CreateCacheLine(processor, text);

        if (!_undefinedCache.TryAdd(id, undefinedHash))
        {
            _logger.LogError("[Cache Repository: concurrent create error - 1]");
        }

        if (!_definedCache.TryAdd(id, definedHash))
        {
            _logger.LogError("[Cache Repository: concurrent create error - 2]");
        }

        _lockSlim.ExitReadLock();
    }

    public void Update(int id, string text)
    {
        _lockSlim.EnterReadLock();

        using var scope = _factory.CreateScope();

        var processor = scope.ServiceProvider.GetRequiredService<ITextProcessor>();

        var (definedHash, undefinedHash, _) = CreateCacheLine(processor, text);

        if (_undefinedCache.TryGetValue(id, out var oldHash))
        {
            if (!_undefinedCache.TryUpdate(id, undefinedHash, oldHash))
            {
                _logger.LogError("[Cache Repository: concurrent update error - 1_2]");
            }
        }
        else
        {
            _logger.LogError("[Cache Repository: concurrent update error - 1_1]");
        }

        if (_definedCache.TryGetValue(id, out oldHash))
        {
            if (!_definedCache.TryUpdate(id, definedHash, oldHash))
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
        _lockSlim.EnterWriteLock();

        using var scope = _factory.CreateScope();

        using var repo = scope.ServiceProvider.GetRequiredService<IDataRepository>();

        var processor = scope.ServiceProvider.GetRequiredService<ITextProcessor>();

        try
        {
            _undefinedCache.Clear();

            _definedCache.Clear();

            var texts = repo.ReadAllSongs();

            foreach (var text in texts)
            {
                var (definedHash, undefinedHash, songNumber) = CreateCacheLine(processor, text);

                if (!_undefinedCache.TryAdd(songNumber, undefinedHash))
                {
                    throw new MethodAccessException("[Cache Repository Init: undefined failed]");
                }

                if (!_definedCache.TryAdd(songNumber, definedHash))
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

    private static (List<int> Def, List<int> Undef, int Num) CreateCacheLine(ITextProcessor processor, string text)
    {
        // undefined hash line
        processor.Setup(ConsonantChain.Undefined);

        var song = processor.ConvertStringToText(text);

        song.Title.ForEach(t => song.Words.Add(t));

        var undefinedHashLine = processor.GetHashSetFromStrings(song.Words);

        // defined hash line
        processor.Setup(ConsonantChain.Defined);

        song = processor.ConvertStringToText(text);

        song.Title.ForEach(t => song.Words.Add(t));

        var definedHashLine = processor.GetHashSetFromStrings(song.Words);

        return (Def: definedHashLine, Undef: undefinedHashLine, Num: song.Number);
    }
}