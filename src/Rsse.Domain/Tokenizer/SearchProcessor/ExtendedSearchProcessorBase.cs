using SearchEngine.Tokenizer.Contracts;
using SearchEngine.Tokenizer.Indexes;

namespace SearchEngine.Tokenizer.SearchProcessor;

/// <summary>
/// Базовый класс для функционала алгоритмов подсчёта extended метрик.
/// </summary>
public abstract class ExtendedSearchProcessorBase
{
    /// <summary>
    /// Фабрика токенизаторов.
    /// </summary>
    public required ITokenizerProcessorFactory TokenizerProcessorFactory { get; init; }

    /// <summary>
    /// Поддержка индекса для всех токенизированных заметок.
    /// </summary>
    public required DirectIndexHandler DirectIndexHandler { get; init; }
}
