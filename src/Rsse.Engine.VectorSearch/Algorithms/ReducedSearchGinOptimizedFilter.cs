using System;
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
        var sortedIds = TempStoragePool.GetDocumentIdCollectionList<TDocumentIdCollection>();
        var removeList = TempStoragePool.DocumentIdListsStorage.Get();

        try
        {
            if (!RelevanceFilter.FindFilteredDocumentsReduced(GinReduced, searchVector, comparisonScores, sortedIds,
                    out var filteredTokensCount))
            {
                return;
            }

            if (cancellationToken.IsCancellationRequested)
                throw new OperationCanceledException(nameof(ReducedSearchGinOptimizedFilter<TDocumentIdCollection>));

            // сразу посчитаем на GIN метрики intersect.count для актуальных идентификаторов
            var counter = 1;
            for (var index = filteredTokensCount; index < sortedIds.Count; index++)
            {
                var documentIds = sortedIds[index];
                ReducedAlgorithm.ComputeComparisonScores(comparisonScores, documentIds, removeList, ref counter);

                counter++;
            }

            // поиск в векторе reduced
            foreach (var (documentId, comparisonScore) in comparisonScores)
            {
                metricsCalculator.AppendReduced(comparisonScore, searchVector, documentId, GeneralDirectIndex);
            }
        }
        finally
        {
            TempStoragePool.DocumentIdListsStorage.Return(removeList);
            TempStoragePool.ReturnDocumentIdCollectionList(sortedIds);
            TempStoragePool.ScoresStorage.Return(comparisonScores);
        }
    }
}
