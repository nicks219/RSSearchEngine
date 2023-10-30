using System.Collections.Concurrent;
using System.Collections.Generic;
using SearchEngine.Data;

namespace SearchEngine.Infrastructure.Tokenizer.Contracts;

public interface ITokenizerService
{
    public ConcurrentDictionary<int, List<int>> GetUndefinedLines();

    public ConcurrentDictionary<int, List<int>> GetDefinedLines();

    public void Delete(int id);

    public void Create(int id, TextEntity text);

    public void Update(int id, TextEntity text);

    public void Initialize();
}
