using System;
using System.Collections.Generic;
using System.Threading;
using RsseEngine.Contracts;
using RsseEngine.Dto;
using RsseEngine.Indexes;
using RsseEngine.Pools;
using RsseEngine.Processor;

namespace RsseEngine.Algorithms;

/// <summary>
/// Класс с алгоритмом подсчёта сокращенной метрики.
/// Метрика считается GIN индексе, применены дополнительные оптимизации.
/// </summary>
public sealed class ReducedSearchGinFastFilter<TDocumentIdCollection> : IReducedSearchProcessor
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
    public void FindReduced(TokenVector searchVector, IMetricsCalculator metricsCalculator,
        CancellationToken cancellationToken)
    {
        var sortedIds = TempStoragePool.GetDocumentIdCollectionList<TDocumentIdCollection>();

        try
        {
            if (!RelevanceFilter.FindFilteredDocumentsReduced(GinReduced, searchVector, sortedIds,
                    out var filteredTokensCount))
            {
                return;
            }

            switch (sortedIds.Count)
            {
                case 0:
                    {
                        break;
                    }
                case 1:
                    {
                        foreach (var documentId in sortedIds[0])
                        {
                            const int metric = 1;
                            metricsCalculator.AppendReduced(metric, searchVector, documentId, GeneralDirectIndex);
                        }

                        break;
                    }
                default:
                    {
                        if (cancellationToken.IsCancellationRequested)
                            throw new OperationCanceledException(nameof(ReducedSearchGinFastFilter<TDocumentIdCollection>));

                        Process(sortedIds, searchVector, metricsCalculator, filteredTokensCount);

                        break;
                    }
            }
        }
        finally
        {
            TempStoragePool.ReturnDocumentIdCollectionList(sortedIds);
        }
    }

    private void Process(List<TDocumentIdCollection> sortedIds, TokenVector searchVector,
        IMetricsCalculator metricsCalculator, int filteredTokensCount)
    {
        var comparisonScores = new ComparisonScores(TempStoragePool.ScoresStorage.Get());
        var removeList = TempStoragePool.DocumentIdListsStorage.Get();

        try
        {
            ReducedAlgorithm.CreateComparisonScores(sortedIds, filteredTokensCount, comparisonScores);

            // сразу посчитаем на GIN метрики intersect.count для актуальных идентификаторов

            var lastIndex = sortedIds.Count - 1;

            var counter = 1;
            for (var index = filteredTokensCount; index < lastIndex; index++)
            {
                var documentIds = sortedIds[index];
                ReducedAlgorithm.ComputeComparisonScores(comparisonScores, documentIds, removeList, ref counter);

                counter++;
            }

            // Отдаём метрику на самый тяжелый токен поискового запроса.
            if (filteredTokensCount < sortedIds.Count)
            {
                var documentIdSet = sortedIds[lastIndex];

                if (comparisonScores.Count <= documentIdSet.Count)
                {
                    foreach (var (documentId, _) in comparisonScores)
                    {
                        if (documentIdSet.Contains(documentId))
                        {
                            removeList.Add(documentId);
                        }
                    }

                    foreach (var documentId in removeList)
                    {
                        IncrementComparisonScore(comparisonScores, documentId, metricsCalculator, searchVector);
                    }
                }
                else
                {
                    foreach (var documentId in documentIdSet)
                    {
                        IncrementComparisonScore(comparisonScores, documentId, metricsCalculator, searchVector);
                    }
                }
            }

            // Поиск в векторе reduced без учета самого тяжелого токена.
            metricsCalculator.AppendReducedMetrics(GeneralDirectIndex, searchVector, comparisonScores);
        }
        finally
        {
            TempStoragePool.DocumentIdListsStorage.Return(removeList);
            TempStoragePool.ScoresStorage.Return(comparisonScores.Dictionary);
        }
    }

    private void IncrementComparisonScore(ComparisonScores comparisonScores, DocumentId documentId,
        IMetricsCalculator metricsCalculator, TokenVector searchVector)
    {
        if (comparisonScores.Remove(documentId, out var score))
        {
            ++score;

            metricsCalculator.AppendReduced(score, searchVector, documentId,
                GeneralDirectIndex);
        }
    }
}
