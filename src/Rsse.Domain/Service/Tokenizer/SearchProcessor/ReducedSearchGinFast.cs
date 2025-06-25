using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using Rsse.Search.Dto;
using Rsse.Search.Indexes;

namespace SearchEngine.Service.Tokenizer.SearchProcessor;

/// <summary>
/// Класс с алгоритмом подсчёта сокращенной метрики.
/// Метрика считается GIN индексе, применены дополнительные оптимизации.
/// </summary>
public sealed class ReducedSearchGinFast : ReducedSearchProcessorBase
{
    /// <summary>
    /// Поддержка GIN-индекса.
    /// </summary>
    public required GinHandler<DocumentIdSet> GinReduced { get; init; }

    protected override void FindReduced(TokenVector reducedSearchVector, MetricsCalculator metricsCalculator, CancellationToken cancellationToken)
    {
        // убираем дубликаты слов для intersect - это меняет результаты поиска (тексты типа "казино казино казино")
        reducedSearchVector = reducedSearchVector.DistinctAndGet();

        // сразу посчитаем на GIN метрики intersect.count для актуальных идентификаторов
        var comparisonScoresReduced = new Dictionary<DocumentId, int>();
        foreach (var token in reducedSearchVector)
        {
            if (!GinReduced.TryGetIdentifiers(token, out var ids))
            {
                continue;
            }

            foreach (var docId in ids)
            {
                ref var score = ref CollectionsMarshal.GetValueRefOrAddDefault(comparisonScoresReduced, docId, out _);
                ++score;
            }
        }

        // поиск в векторе reduced
        if (cancellationToken.IsCancellationRequested) throw new OperationCanceledException(nameof(ReducedSearchGinOptimized));
        foreach (var (docId, comparisonScore) in comparisonScoresReduced)
        {
            var reducedTargetVector = GeneralDirectIndex[docId].Reduced;

            metricsCalculator.AppendReduced(comparisonScore, reducedSearchVector, docId, reducedTargetVector);
        }
    }
}
