using System.Collections.Concurrent;
using SearchEngine.Tokenizer.Dto;

namespace SearchEngine.Tokenizer.Indexes;

/// <summary>
/// Поддержка общего индекса по идентификаторам.
/// </summary>
public sealed class DirectIndexHandler
{
    /// <summary>
    /// Получить индекс: идентификатор заметки - токены.
    /// </summary>
    public ConcurrentDictionary<DocId, TokenVectors> GetGeneralDirectIndex { get; private set; } = new();
}
