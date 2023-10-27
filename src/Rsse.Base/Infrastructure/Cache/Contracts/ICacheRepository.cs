using System.Collections.Concurrent;
using System.Collections.Generic;
using SearchEngine.Data;

namespace SearchEngine.Infrastructure.Cache.Contracts;

public interface ICacheRepository
{
    public ConcurrentDictionary<int, List<int>> GetUndefinedCache();

    public ConcurrentDictionary<int, List<int>> GetDefinedCache();

    public void Delete(int id);

    public void Create(int id, TextEntity text);

    public void Update(int id, TextEntity text);

    public void Initialize();
}
