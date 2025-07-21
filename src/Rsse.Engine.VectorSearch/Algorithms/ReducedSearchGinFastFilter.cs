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
        // убираем дубликаты слов для intersect - это меняет результаты поиска (тексты типа "казино казино казино")
        searchVector = searchVector.DistinctAndGet();

        var comparisonScores = TempStoragePool.ScoresStorage.Get();
        var sortedIds = TempStoragePool.GetDocumentIdCollectionList<TDocumentIdCollection>();
        var removeList = TempStoragePool.DocumentIdListsStorage.Get();

        try
        {
            if (!RelevanceFilter.FindFilteredDocumentsReduced(GinReduced, searchVector, sortedIds,
                    out var filteredTokensCount, comparisonScores))
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
                            metricsCalculator.AppendReduced(1, searchVector, documentId, GeneralDirectIndex);
                        }

                        break;
                    }
                default:
                    {
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

                        if (cancellationToken.IsCancellationRequested)
                            throw new OperationCanceledException(nameof(ReducedSearchGinFast<TDocumentIdCollection>));

                        // Поиск в векторе reduced без учета самого тяжелого токена.
                        foreach (var (documentId, comparisonScore) in comparisonScores)
                        {
                            metricsCalculator.AppendReduced(comparisonScore, searchVector, documentId,
                                GeneralDirectIndex);
                        }

                        break;
                    }
            }
        }
        finally
        {
            TempStoragePool.DocumentIdListsStorage.Return(removeList);
            TempStoragePool.ReturnDocumentIdCollectionList(sortedIds);
            TempStoragePool.ScoresStorage.Return(comparisonScores);
        }
    }

    private void IncrementComparisonScore(Dictionary<DocumentId, int> comparisonScores, DocumentId documentId,
        IMetricsCalculator metricsCalculator, TokenVector searchVector)
    {
        if (comparisonScores.Remove(documentId, out var comparisonScore))
        {
            ++comparisonScore;

            metricsCalculator.AppendReduced(comparisonScore, searchVector, documentId,
                GeneralDirectIndex);
        }
    }
}
