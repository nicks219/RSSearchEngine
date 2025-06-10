using System;
using System.Collections.Generic;
using System.Threading;
using SearchEngine.Service.Tokenizer.Processor;
using SearchEngine.Service.Tokenizer.Wrapper;

namespace SearchEngine.Service.Tokenizer.Factory;

/// <summary>
/// Класс с алгоритмом подсчёта сокращенной метрики.
/// Метрика считается с помощью GIN индекса в процессе поиска.
/// </summary>
public class ReducedMetricsGin : ReducedMetricsBase, IReducedMetricsProcessor
{
    /// <inheritdoc/>
    public void FindReduced(string text, Dictionary<DocId, double> complianceMetrics, CancellationToken cancellationToken)
    {
        var processor = ProcessorFactory.CreateProcessor(ProcessorType.Reduced);

        var reducedSearchVector = processor.TokenizeText(text);

        if (reducedSearchVector.Count == 0)
        {
            // песни вида "123 456" не ищем, так как получим весь каталог
            return;
        }

        // убираем дубликаты слов для intersect - это меняет результаты поиска (тексты типа "казино казино казино")
        reducedSearchVector = reducedSearchVector.DistinctAndGet();

        // поиск в векторе reduced
        if (cancellationToken.IsCancellationRequested) throw new OperationCanceledException(nameof(ReducedMetricsGin));
        foreach (var (docId, tokenLine) in TokenLines)
        {
            var reducedTargetVectorCount = tokenLine.Reduced.Count;
            var comparisonScore = 0;
            foreach (var token in reducedSearchVector)
            {
                if (ReducedGin.TryGetIdentifiers(token, out var reducedTokens) && reducedTokens.Contains(docId))
                {
                    comparisonScore++;
                }
            }

            // III. 100% совпадение по reduced
            if (comparisonScore == reducedSearchVector.Count)
            {
                complianceMetrics.TryAdd(docId, comparisonScore * (10D / reducedTargetVectorCount));
                continue;
            }

            // IV. reduced% совпадение - мы не можем наверняка оценить неточное совпадение
            if (comparisonScore >= reducedSearchVector.Count * ReducedCoefficient)
            {
                complianceMetrics.TryAdd(docId, comparisonScore * (1D / reducedTargetVectorCount));
            }
        }
    }
}
