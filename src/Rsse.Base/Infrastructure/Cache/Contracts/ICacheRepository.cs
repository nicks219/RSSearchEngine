using System.Collections.Concurrent;
using RandomSongSearchEngine.Data;

namespace RandomSongSearchEngine.Infrastructure.Cache.Contracts;

public interface ICacheRepository
{
    public ConcurrentDictionary<int, List<int>> GetUndefinedCache();
    
    public ConcurrentDictionary<int, List<int>> GetDefinedCache();

    public void Delete(int id);

    public void Create(int id, TextEntity text);

    public void Update(int id, TextEntity text);

    public void Initialize();
}