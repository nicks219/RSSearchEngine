using System.Collections.Concurrent;
using System.Threading;
using Rsse.Search.Dto;
using SearchEngine.Service.Tokenizer.Contracts;
using SearchEngine.Service.Tokenizer.Dto;
using SearchEngine.Service.Tokenizer.TokenizerProcessor;

namespace SearchEngine.Service.Tokenizer.SearchProcessor;

/// <summary>
/// Базовый класс для функционала алгоритмов подсчёта extended метрик.
/// </summary>
public abstract class ExtendedSearchProcessorBase : IExtendedSearchProcessor
{
    /// <summary>
    /// Фабрика токенизаторов.
    /// </summary>
    public required ITokenizerProcessorFactory TokenizerProcessorFactory { get; init; }

    /// <summary>
    /// Индекс для всех токенизированных заметок.
    /// </summary>
    public required ConcurrentDictionary<DocumentId, TokenLine> GeneralDirectIndex { get; init; }

    /// <inheritdoc/>
    public bool FindExtended(string text, MetricsCalculator metricsCalculator, CancellationToken cancellationToken)
    {
        var processor = TokenizerProcessorFactory.CreateProcessor(ProcessorType.Extended);

        var extendedSearchVector = processor.TokenizeText(text);

        if (extendedSearchVector.Count == 0)
        {
            // заметки вида "123 456" не ищем, так как получим весь каталог
            return false;
        }

        FindExtended(extendedSearchVector, metricsCalculator, cancellationToken);

        return true;
    }

    protected abstract void FindExtended(TokenVector extendedSearchVector, MetricsCalculator metricsCalculator,
        CancellationToken cancellationToken);
}
