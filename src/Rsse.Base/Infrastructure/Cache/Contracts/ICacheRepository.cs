using System.Collections.Concurrent;

namespace RandomSongSearchEngine.Infrastructure.Cache.Contracts;

public interface ICacheRepository
{
    public ConcurrentDictionary<int, List<int>> GetUndefinedCache();
    
    public ConcurrentDictionary<int, List<int>> GetDefinedCache();

    public void Delete(int id);

    public void Create(int id, string text);

    public void Update(int id, string text);

    public void Initialize();
}