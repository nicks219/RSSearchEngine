using System.Collections.Concurrent;
using System.Threading;
using Rsse.Search.Dto;
using SearchEngine.Service.Tokenizer.Contracts;
using SearchEngine.Service.Tokenizer.Dto;
using SearchEngine.Service.Tokenizer.TokenizerProcessor;

namespace SearchEngine.Service.Tokenizer.SearchProcessor;

/// <summary>
/// Базовый класс для функционала алгоритмов подсчёта reduced метрик.
/// </summary>
public abstract class ReducedSearchProcessorBase : IReducedSearchProcessor
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
    public void FindReduced(string text, MetricsCalculator metricsCalculator, CancellationToken cancellationToken)
    {
        var processor = TokenizerProcessorFactory.CreateProcessor(ProcessorType.Reduced);

        TokenVector reducedSearchVector = processor.TokenizeText(text);

        if (reducedSearchVector.Count == 0)
        {
            // песни вида "123 456" не ищем, так как получим весь каталог
            return;
        }

        FindReduced(reducedSearchVector, metricsCalculator, cancellationToken);
    }

    protected abstract void FindReduced(TokenVector reducedSearchVector, MetricsCalculator metricsCalculator, CancellationToken cancellationToken);
}
