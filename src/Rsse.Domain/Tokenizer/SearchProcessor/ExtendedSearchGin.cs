using System;
using System.Threading;
using SearchEngine.Tokenizer.Contracts;
using SearchEngine.Tokenizer.Indexes;
using SearchEngine.Tokenizer.TokenizerProcessor;

namespace SearchEngine.Tokenizer.SearchProcessor;

/// <summary>
/// Класс с алгоритмом подсчёта расширенной метрики.
/// Пространство поиска сокращается с помощью выбора из GIN индекса.
/// </summary>
public sealed class ExtendedSearchGin : ExtendedSearchProcessorBase, IExtendedSearchProcessor
{
    /// <summary>
    /// Поддержка GIN-индекса.
    /// </summary>
    public required InvertedIndexHandler InvertedIndexExtended { get; init; }

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
        var directIndex = DirectIndexHandler.GetGeneralDirectIndex;
        foreach (var (docId, tokenLine) in directIndex)
        {
            if (!InvertedIndexExtended.ContainsAnyTokenForDoc(extendedSearchVector, docId))
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
