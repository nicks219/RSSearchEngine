using System;
using System.Collections.Generic;
using System.Threading;
using SearchEngine.Service.Tokenizer.Processor;
using SearchEngine.Service.Tokenizer.Wrapper;

namespace SearchEngine.Service.Tokenizer.Factory;

/// <summary>
/// Класс с алгоритмом подсчёта расширенной метрики.
/// Пространство поиска сокращается с помощью выбора из GIN индекса.
/// </summary>
public class ExtendedMetricsGin : ExtendedMetricsBase, IExtendedMetricsProcessor
{
    /// <inheritdoc/>
    public bool FindExtended(string text, Dictionary<DocId, double> complianceMetrics, CancellationToken cancellationToken)
    {
        var continueSearching = true;

        var processor = ProcessorFactory.CreateProcessor(ProcessorType.Extended);

        var extendedSearchVector = processor.TokenizeText(text);

        if (extendedSearchVector.Count == 0)
        {
            // заметки вида "123 456" не ищем, так как получим весь каталог
            return false;
        }

        // поиск в векторе extended
        if (cancellationToken.IsCancellationRequested) throw new OperationCanceledException(nameof(ExtendedMetricsGin));
        foreach (var (docId, tokenLine) in TokenLines)
        {
            if (!ExtendedGin.ContainsAnyTokenInId(extendedSearchVector, docId))
            {
                continue;
            }

            var extendedTargetVector = tokenLine.Extended;
            var comparisonScore = processor.ComputeComparisonScore(extendedTargetVector, extendedSearchVector);

            // I. 100% совпадение по extended последовательности, по reduced можно не искать
            if (comparisonScore == extendedSearchVector.Count)
            {
                continueSearching = false;
                complianceMetrics.Add(docId, comparisonScore * (1000D / extendedTargetVector.Count));
                continue;
            }

            // II. extended% совпадение
            if (comparisonScore >= extendedSearchVector.Count * ExtendedCoefficient)
            {
                // todo: можно так оценить
                // continueSearching = false;
                complianceMetrics.Add(docId, comparisonScore * (100D / extendedTargetVector.Count));
            }
        }

        return continueSearching;
    }
}
