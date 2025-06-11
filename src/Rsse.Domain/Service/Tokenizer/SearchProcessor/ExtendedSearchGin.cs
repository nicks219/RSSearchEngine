using System;
using System.Threading;
using SearchEngine.Service.Tokenizer.Contracts;
using SearchEngine.Service.Tokenizer.Indexes;
using SearchEngine.Service.Tokenizer.TokenizerProcessor;

namespace SearchEngine.Service.Tokenizer.SearchProcessor;

/// <summary>
/// Класс с алгоритмом подсчёта расширенной метрики.
/// Пространство поиска сокращается с помощью выбора из GIN индекса.
/// </summary>
public class ExtendedSearchGin : ExtendedSearchProcessorBase, IExtendedSearchProcessor
{
    /// <summary>
    /// Поддержка GIN-индекса.
    /// </summary>
    public required GinHandler GinExtended { get; init; }

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

        // поиск в векторе extended
        if (cancellationToken.IsCancellationRequested) throw new OperationCanceledException(nameof(ExtendedSearchGin));
        foreach (var (docId, tokenLine) in GeneralDirectIndex)
        {
            if (!GinExtended.ContainsAnyTokenForDoc(extendedSearchVector, docId))
            {
                continue;
            }

            var extendedTargetVector = tokenLine.Extended;
            var comparisonScore = processor.ComputeComparisonScore(extendedTargetVector, extendedSearchVector);

            metricsCalculator.AppendExtended(comparisonScore, extendedSearchVector, docId, extendedTargetVector);
        }

        return true;
    }
}
