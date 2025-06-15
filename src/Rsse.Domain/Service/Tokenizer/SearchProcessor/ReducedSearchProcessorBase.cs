using System.Collections.Concurrent;
using SearchEngine.Service.Tokenizer.Contracts;
using SearchEngine.Service.Tokenizer.Dto;

namespace SearchEngine.Service.Tokenizer.SearchProcessor;

/// <summary>
/// Базовый класс для функционала алгоритмов подсчёта reduced метрик.
/// </summary>
public class ReducedSearchProcessorBase
{
    /// <summary>
    /// Фабрика токенизаторов.
    /// </summary>
    public required ITokenizerProcessorFactory TokenizerProcessorFactory { get; init; }

    /// <summary>
    /// Индекс для всех токенизированных заметок.
    /// </summary>
    public required ConcurrentDictionary<DocId, TokenLine> GeneralDirectIndex { get; init; }
}
