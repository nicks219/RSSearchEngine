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
        var sortedIds = TempStoragePool.GetDocumentIdCollectionList<TDocumentIdCollection>();

        try
        {
            if (!RelevanceFilter.FindFilteredDocumentsReduced(GinReduced, searchVector, sortedIds,
                    out var filteredTokensCount))
            {
                return;
            }

            if (cancellationToken.IsCancellationRequested)
                throw new OperationCanceledException(nameof(ReducedSearchGinOptimizedFilter<TDocumentIdCollection>));

            var comparisonScores = new ComparisonScores(TempStoragePool.ScoresStorage.Get());
            var removeList = TempStoragePool.DocumentIdListsStorage.Get();

            try
            {
                ReducedAlgorithm.CreateComparisonScores(sortedIds, filteredTokensCount, comparisonScores);

                // сразу посчитаем на GIN метрики intersect.count для актуальных идентификаторов
                var counter = 1;
                for (var index = filteredTokensCount; index < sortedIds.Count; index++)
                {
                    var documentIds = sortedIds[index];
                    ReducedAlgorithm.ComputeComparisonScores(comparisonScores, documentIds, removeList, ref counter);

                    counter++;
                }

                // поиск в векторе reduced
                metricsCalculator.AppendReducedMetrics(GeneralDirectIndex,searchVector, comparisonScores);
            }
            finally
            {
                TempStoragePool.DocumentIdListsStorage.Return(removeList);
                TempStoragePool.ScoresStorage.Return(comparisonScores.Dictionary);
            }
        }
        finally
        {
            TempStoragePool.ReturnDocumentIdCollectionList(sortedIds);
        }
    }
}
