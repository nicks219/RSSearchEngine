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
public sealed class ReducedSearchGinOptimizedFilter<TDocumentIdCollection> : IReducedSearchProcessor
    where TDocumentIdCollection : struct, IDocumentIdCollection<TDocumentIdCollection>
{
    public required TempStoragePool TempStoragePool { private get; init; }

    /// <summary>
    /// Общий индекс: идентификатор-вектор.
    /// </summary>
    public required DirectIndex GeneralDirectIndex { private get; init; }

    /// <summary>
    /// Общий инвертированный индекс: токен-идентификаторы.
    /// </summary>
    public required InvertedIndex<TDocumentIdCollection> GinReduced { private get; init; }

    public required GinRelevanceFilter RelevanceFilter { private get; init; }

    /// <inheritdoc/>
    public void FindReduced(TokenVector searchVector, IMetricsCalculator metricsCalculator, CancellationToken cancellationToken)
    {
        // убираем дубликаты слов для intersect - это меняет результаты поиска (тексты типа "казино казино казино")
        searchVector = searchVector.DistinctAndGet();

        var comparisonScores = TempStoragePool.ScoresStorage.Get();
        var idsFromGin = TempStoragePool.GetDocumentIdCollectionList<TDocumentIdCollection>();

        try
        {
            if (!RelevanceFilter.FindFilteredDocumentsReduced(GinReduced, searchVector, comparisonScores, idsFromGin))
            {
                return;
            }

            if (cancellationToken.IsCancellationRequested)
                throw new OperationCanceledException(nameof(ReducedSearchGinOptimizedFilter<TDocumentIdCollection>));

            // сразу посчитаем на GIN метрики intersect.count для актуальных идентификаторов
            foreach (var documentIds in idsFromGin)
            {
                if (comparisonScores.Count < documentIds.Count)
                {
                    foreach (var (documentId, _) in comparisonScores)
                    {
                        if (documentIds.Contains(documentId))
                        {
                            IncrementCounter(comparisonScores, documentId);
                        }
                    }
                }
                else
                {
                    foreach (var documentId in documentIds)
                    {
                        IncrementCounter(comparisonScores, documentId);
                    }
                }
            }

            // поиск в векторе reduced
            foreach (var (documentId, comparisonScore) in comparisonScores)
            {
                metricsCalculator.AppendReduced(comparisonScore, searchVector, documentId, GeneralDirectIndex);
            }
        }
        finally
        {
            TempStoragePool.ReturnDocumentIdCollectionList(idsFromGin);
            TempStoragePool.ScoresStorage.Return(comparisonScores);
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
