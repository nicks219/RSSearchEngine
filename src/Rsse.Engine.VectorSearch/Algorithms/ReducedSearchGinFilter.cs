using System;
using System.Threading;
using RsseEngine.Contracts;
using RsseEngine.Dto;
using RsseEngine.Indexes;
using RsseEngine.Pools;
using RsseEngine.Processor;

namespace RsseEngine.Algorithms;

/// <summary>
/// Класс с алгоритмом подсчёта расширенной метрики.
/// Пространство поиска формируется с помощью GIN индекса.
/// </summary>
public sealed class ReducedSearchGinFilter<TDocumentIdCollection> : IReducedSearchProcessor
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

        var sortedIds = TempStoragePool.GetDocumentIdCollectionList<TDocumentIdCollection>();

        try
        {
            if (!RelevanceFilter.FindFilteredDocumentsReduced(GinReduced, searchVector, sortedIds,
                    out var filteredTokensCount))
            {
                return;
            }

            if (cancellationToken.IsCancellationRequested)
                throw new OperationCanceledException(nameof(ReducedSearchGinFilter<TDocumentIdCollection>));

            var comparisonScores = new ComparisonScores(TempStoragePool.ScoresStorage.Get());

            try
            {
                ReducedAlgorithm.CreateComparisonScores(sortedIds, filteredTokensCount, comparisonScores);

                // поиск в векторе reduced
                foreach (var (documentId, _) in comparisonScores)
                {
                    metricsCalculator.AppendReducedMetric(searchVector, documentId, GeneralDirectIndex);
                }
            }
            finally
            {
                TempStoragePool.ScoresStorage.Return(comparisonScores.Dictionary);
            }
        }
        finally
        {
            TempStoragePool.ReturnDocumentIdCollectionList(sortedIds);
        }
    }
}
