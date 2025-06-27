using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using Rsse.Search.Dto;
using Rsse.Search.Indexes;

namespace Rsse.Search.Algorithms;

/// <summary>
/// Класс с алгоритмом подсчёта сокращенной метрики.
/// Метрика считается GIN индексе, применены дополнительные оптимизации.
/// </summary>
public sealed class ReducedSearchGinFast : IReducedSearchProcessor
{
    /// <summary>
    /// Индекс для всех токенизированных заметок.
    /// </summary>
    public required DirectIndex GeneralDirectIndex { get; init; }

    /// <summary>
    /// Поддержка GIN-индекса.
    /// </summary>
    public required InverseIndex<DocumentIdSet> GinReduced { get; init; }

    /// <inheritdoc/>
    public void FindReduced(TokenVector searchVector, IMetricsCalculator metricsCalculator, CancellationToken cancellationToken)
    {
        // убираем дубликаты слов для intersect - это меняет результаты поиска (тексты типа "казино казино казино")
        searchVector = searchVector.DistinctAndGet();

        // сразу посчитаем на GIN метрики intersect.count для актуальных идентификаторов
        var comparisonScoresReduced = new Dictionary<DocumentId, int>();
        foreach (var token in searchVector)
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

        if (cancellationToken.IsCancellationRequested) throw new OperationCanceledException(nameof(ReducedSearchGinOptimized));

        // поиск в векторе reduced
        foreach (var (docId, comparisonScore) in comparisonScoresReduced)
        {
            var reducedTargetVector = GeneralDirectIndex[docId].Reduced;

            metricsCalculator.AppendReduced(comparisonScore, searchVector, docId, reducedTargetVector);
        }
    }
}
