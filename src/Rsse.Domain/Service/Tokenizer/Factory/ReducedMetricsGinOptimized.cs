using System;
using System.Collections.Generic;
using System.Threading;
using SearchEngine.Service.Tokenizer.Processor;
using SearchEngine.Service.Tokenizer.Wrapper;

namespace SearchEngine.Service.Tokenizer.Factory;

/// <summary>
/// Класс с алгоритмом подсчёта сокращенной метрики.
/// Метрика считается на самом GIN индексе и создаётся промежуточный результат для поиска в нём.
/// </summary>
public class ReducedMetricsGinOptimized : ReducedMetricsBase, IReducedMetricsProcessor
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

        // сразу посчитаем на GIN метрики intersect.count для актуальных идентификаторов
        var comparisonScoresReduced = new Dictionary<DocId, int>();
        foreach (var token in reducedSearchVector)
        {
            if (!ReducedGin.TryGetIdentifiers(token, out var ids))
            {
                continue;
            }

            foreach (var docId in ids)
            {
                if (!comparisonScoresReduced.TryAdd(docId, 1))
                {
                    // Это метрика intersect.count.
                    comparisonScoresReduced[docId]++;
                }
            }
        }

        // поиск в векторе reduced
        if (cancellationToken.IsCancellationRequested) throw new OperationCanceledException(nameof(ReducedMetricsGinOptimized));
        foreach (var (docId, comparisonScore) in comparisonScoresReduced)
        {
            // Нужен только count.
            var reducedTargetVectorCount = TokenLines[docId].Reduced.Count;

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
