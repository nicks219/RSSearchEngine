using System;
using System.Collections.Generic;
using System.Linq;
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
/// Метрика считается GIN индексе, применены дополнительные оптимизации.
/// </summary>
public sealed class ReducedSearchGinFastFilter : IReducedSearchProcessor
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

        var idsFromGin = new List<DocumentIdSet>();

        foreach (var token in searchVector)
        {
            if (GinReduced.TryGetNonEmptyDocumentIdVector(token, out var ids))
            {
                idsFromGin.Add(ids);
            }
        }

        switch (idsFromGin.Count)
        {
            case 0:
                {
                    break;
                }
            case 1:
                {
                    foreach (var docId in idsFromGin[0])
                    {
                        metricsCalculator.AppendReduced(1, searchVector, docId, GeneralDirectIndex);
                    }

                    break;
                }
            default:
                {
                    var filteredDocuments = RelevanceFilter.ProcessToSet(GinReduced, searchVector);
                    if (filteredDocuments.Count == 0)
                    {
                        return;
                    }

                    idsFromGin = idsFromGin.OrderBy(docIdVector => docIdVector.Count).ToList();

                    // сразу посчитаем на GIN метрики intersect.count для актуальных идентификаторов
                    var comparisonScoresReduced = TempStoragePool.ScoresTempStorage.Get();

                    try
                    {
                        var lastIndex = idsFromGin.Count - 1;

                        for (var index = 0; index < lastIndex; index++)
                        {
                            DocumentIdSet documentIdSet = idsFromGin[index];

                            if (filteredDocuments.Count <= documentIdSet.Count)
                            {
                                foreach (var docId in filteredDocuments)
                                {
                                    if (!documentIdSet.Contains(docId))
                                    {
                                        continue;
                                    }

                                    ref var score = ref CollectionsMarshal.GetValueRefOrAddDefault(comparisonScoresReduced,
                                        docId, out _);

                                    ++score;
                                }
                            }
                            else
                            {
                                foreach (var docId in documentIdSet)
                                {
                                    if (!filteredDocuments.Contains(docId))
                                    {
                                        continue;
                                    }

                                    ref var score = ref CollectionsMarshal.GetValueRefOrAddDefault(comparisonScoresReduced,
                                        docId, out _);

                                    ++score;
                                }
                            }
                        }

                        // Отдаём метрику на самый тяжелый токен поискового запроса.
                        {
                            DocumentIdSet documentIdSet = idsFromGin[lastIndex];

                            if (filteredDocuments.Count <= documentIdSet.Count)
                            {
                                foreach (var docId in filteredDocuments)
                                {
                                    if (!documentIdSet.Contains(docId))
                                    {
                                        continue;
                                    }

                                    comparisonScoresReduced.Remove(docId, out var comparisonScore);
                                    ++comparisonScore;

                                    metricsCalculator.AppendReduced(comparisonScore, searchVector, docId,
                                        GeneralDirectIndex);
                                }
                            }
                            else
                            {
                                foreach (var docId in documentIdSet)
                                {
                                    if (!filteredDocuments.Contains(docId))
                                    {
                                        continue;
                                    }

                                    comparisonScoresReduced.Remove(docId, out var comparisonScore);
                                    ++comparisonScore;

                                    metricsCalculator.AppendReduced(comparisonScore, searchVector, docId,
                                        GeneralDirectIndex);
                                }
                            }
                        }

                        if (cancellationToken.IsCancellationRequested)
                            throw new OperationCanceledException(nameof(ReducedSearchGinFast));

                        // Поиск в векторе reduced без учета самого тяжелого токена.
                        foreach (var (docId, comparisonScore) in comparisonScoresReduced)
                        {
                            /*if (!filteredDocuments.Contains(docId))
                            {
                                continue;
                            }*/

                            metricsCalculator.AppendReduced(comparisonScore, searchVector, docId,
                                GeneralDirectIndex);
                        }
                    }
                    finally
                    {
                        TempStoragePool.ScoresTempStorage.Return(comparisonScoresReduced);
                    }

                    break;
                }
        }
    }
}
