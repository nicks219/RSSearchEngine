using System.Collections.Concurrent;
using System.Collections.Generic;
using SearchEngine.Data.Entities;

namespace SearchEngine.Infrastructure.Tokenizer.Contracts;

public interface ITokenizerService
{
    public ConcurrentDictionary<int, List<int>> GetReducedLines();

    public ConcurrentDictionary<int, List<int>> GetExtendedLines();

    public void Delete(int id);

    public void Create(int id, NoteEntity note);

    public void Update(int id, NoteEntity note);

    public void Initialize();
}
