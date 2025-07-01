using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using RsseEngine.Contracts;
using RsseEngine.Dto;
using RsseEngine.Indexes;
using RsseEngine.Pools;
using RsseEngine.Processor;

namespace RsseEngine.Algorithms;

/// <summary>
/// Класс с алгоритмом подсчёта сокращенной метрики.
/// Метрика считается с помощью GIN индекса в процессе поиска.
/// </summary>
public sealed class ReducedSearchGinOptimizedFilter : IReducedSearchProcessor
{
    public required TempStoragePool TempStoragePool { private get; init; }

    /// <summary>
    /// Общий индекс: идентификатор-вектор.
    /// </summary>
    public required DirectIndex GeneralDirectIndex { private get; init; }

    /// <summary>
    /// Общий инвертированный индекс: токен-идентификаторы.
    /// </summary>
    public required InvertedIndex<DocumentIdSet> GinReduced { private get; init; }

    public required GinRelevanceFilter RelevanceFilter { private get; init; }

    /// <inheritdoc/>
    public void FindReduced(TokenVector searchVector, IMetricsCalculator metricsCalculator, CancellationToken cancellationToken)
    {
        // убираем дубликаты слов для intersect - это меняет результаты поиска (тексты типа "казино казино казино")
        searchVector = searchVector.DistinctAndGet();

        var filteredDocuments = RelevanceFilter.ProcessToDictionary(GinReduced, searchVector);
        if (filteredDocuments.Dictionary.Count == 0)
        {
            return;
        }

        if (cancellationToken.IsCancellationRequested)
            throw new OperationCanceledException(nameof(ReducedSearchGinOptimizedFilter));

        var comparisonScoresReduced = filteredDocuments.Dictionary;

        // сразу посчитаем на GIN метрики intersect.count для актуальных идентификаторов
        foreach (var ids in filteredDocuments.List)
        {
            if (comparisonScoresReduced.Count < ids.Count)
            {
                foreach (var (documentId, _) in comparisonScoresReduced)
                {
                    if (ids.Contains(documentId))
                    {
                        IncrementCounter(comparisonScoresReduced, documentId);
                    }
                }
            }
            else
            {
                foreach (var documentId in ids)
                {
                    IncrementCounter(comparisonScoresReduced, documentId);
                }
            }
        }

        // поиск в векторе reduced
        foreach (var (documentId, comparisonScore) in comparisonScoresReduced)
        {
            metricsCalculator.AppendReduced(comparisonScore, searchVector, documentId, GeneralDirectIndex);
        }
    }

    private void IncrementCounter(Dictionary<DocumentId, int> comparisonScoresReduced, DocumentId documentId)
    {
        ref var score = ref CollectionsMarshal.GetValueRefOrNullRef(comparisonScoresReduced, documentId);

        if (!Unsafe.IsNullRef(ref score))
        {
            ++score;
        }
    }
}
