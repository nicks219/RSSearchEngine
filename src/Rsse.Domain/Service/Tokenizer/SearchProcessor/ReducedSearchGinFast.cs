using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using SearchEngine.Service.Tokenizer.Contracts;
using SearchEngine.Service.Tokenizer.Dto;
using SearchEngine.Service.Tokenizer.Indexes;
using SearchEngine.Service.Tokenizer.TokenizerProcessor;

namespace SearchEngine.Service.Tokenizer.SearchProcessor;

/// <summary>
/// Класс с алгоритмом подсчёта сокращенной метрики.
/// Метрика считается GIN индексе, применены дополнительные оптимизации.
/// </summary>
public sealed class ReducedSearchGinFast : ReducedSearchProcessorBase, IReducedSearchProcessor
{
    /// <summary>
    /// Поддержка GIN-индекса.
    /// </summary>
    public required GinHandler GinReduced { get; init; }

    /// <inheritdoc/>
    public void FindReduced(string text, MetricsCalculator metricsCalculator, CancellationToken cancellationToken)
    {
        var processor = TokenizerProcessorFactory.CreateProcessor(ProcessorType.Reduced);

        var reducedSearchVector = processor.TokenizeText(text);

        if (reducedSearchVector.Count == 0)
        {
            // песни вида "123 456" не ищем, так как получим весь каталог
            return;
        }

        // убираем дубликаты слов для intersect - это меняет результаты поиска (тексты типа "казино казино казино")
        reducedSearchVector = reducedSearchVector.DistinctAndGet();

        // сразу посчитаем на GIN метрики intersect.count для актуальных идентификаторов
        var maxScore = reducedSearchVector.Count;
        var threshold = (int)(maxScore * MetricsCalculator.ReducedCoefficient) + 1;

        var comparisonScoresReduced = new Dictionary<DocId, int>();

        var vector = new List<DocIdVector>();
        foreach (var token in reducedSearchVector._vector)
        {
            if (!GinReduced.TryGetIdentifiers(new Token(token), out var ids))
            {
                continue;
            }

            vector.Add(ids);
        }

        //vector = vector.OrderBy(t => t._vector.Count).ToList();

        for (var index = 0; index < vector.Count - 1; index++)
        {
            foreach (var docId in vector[index])
            {
                ref var score = ref CollectionsMarshal.GetValueRefOrAddDefault(comparisonScoresReduced, docId, out _);
                ++score;
            }
        }

        for (var index = vector.Count - 1; index < vector.Count; index++)
        {
            foreach (var docId in vector[index])
            {
                comparisonScoresReduced.Remove(docId, out var comparisonScore);
                ++comparisonScore;

                if (comparisonScore == reducedSearchVector.Count ||
                    comparisonScore >= reducedSearchVector.Count * MetricsCalculator.ReducedCoefficient)
                {
                    var reducedTargetVector = GeneralDirectIndex[docId].Reduced;

                    metricsCalculator.AppendReduced(comparisonScore, reducedSearchVector, docId, reducedTargetVector);
                }
            }
        }

        // поиск в векторе reduced
        if (cancellationToken.IsCancellationRequested) throw new OperationCanceledException(nameof(ReducedSearchGinOptimized));
        // с сортировкой: 4550 | без сортировки: 4550 - 0.0035..36 610 кб
        foreach (var (docId, comparisonScore) in comparisonScoresReduced)
        {
            if (comparisonScore == reducedSearchVector.Count ||
                comparisonScore >= reducedSearchVector.Count * MetricsCalculator.ReducedCoefficient)
            {
                var reducedTargetVector = GeneralDirectIndex[docId].Reduced;

                metricsCalculator.AppendReduced(comparisonScore, reducedSearchVector, docId, reducedTargetVector);
            }
        }

        comparisonScoresReduced.Clear();
    }
}

// --- --- ---
/*if (docTokenMatchCounts.TryGetValue(docId, out var existing))
{
    docTokenMatchCounts[docId] = existing + 1;
}
else if (pendingDocTokenCounts.TryGetValue(docId, out var temp))
{
    temp++;
    if (temp == threshold)
    {
        pendingDocTokenCounts.Remove(docId);
        docTokenMatchCounts[docId] = threshold;
    }
    else
    {
        pendingDocTokenCounts[docId] = temp;
    }
}
else
{
    pendingDocTokenCounts[docId] = 1;
}*/
