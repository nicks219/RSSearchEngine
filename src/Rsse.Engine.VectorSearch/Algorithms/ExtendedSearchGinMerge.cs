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
/// Класс с алгоритмом подсчёта расширенной метрики.
/// Пространство поиска формируется с помощью GIN индекса, применены дополнительные оптимизации.
/// </summary>
public sealed class ExtendedSearchGinMerge : IExtendedSearchProcessor
{
    public required TempStoragePool TempStoragePool { private get; init; }

    /// <summary>
    /// Индекс для всех токенизированных заметок.
    /// </summary>
    public required DirectIndex GeneralDirectIndex { get; init; }

    /// <summary>
    /// Поддержка GIN-индекса.
    /// </summary>
    public required InvertedIndex<DocumentIdList> GinExtended { get; init; }

    public required GinRelevanceFilter RelevanceFilter { private get; init; }

    public required bool EnableRelevanceFilter { private get; init; }

    /// <inheritdoc/>
    public void FindExtended(TokenVector searchVector, IMetricsCalculator metricsCalculator,
        CancellationToken cancellationToken)
    {
        if (EnableRelevanceFilter)
        {
            FindExtendedWithFilter(searchVector, metricsCalculator, cancellationToken);
        }
        else
        {
            FindExtendedNoFilter(searchVector, metricsCalculator, cancellationToken);
        }
    }

    private void FindExtendedNoFilter(TokenVector searchVector, IMetricsCalculator metricsCalculator,
        CancellationToken cancellationToken)
    {
        var idsFromGin = TempStoragePool.GetDocumentIdCollectionList<DocumentIdList>();

        try
        {
            GinExtended.GetDocumentIdVectorsToList(searchVector, idsFromGin);

            switch (idsFromGin.Count(vector => vector.Count > 0))
            {
                case 0:
                    {
                        break;
                    }
                case 1:
                    {
                        var idFromGin = idsFromGin.First(vector => vector.Count > 0);

                        foreach (var documentId in idFromGin)
                        {
                            metricsCalculator.AppendExtended(1, searchVector, documentId, GeneralDirectIndex);
                        }

                        break;
                    }
                default:
                    {
                        if (cancellationToken.IsCancellationRequested)
                            throw new OperationCanceledException(nameof(ExtendedSearchGinMerge));

                        CreateExtendedSearchSpace(searchVector, metricsCalculator, idsFromGin);

                        break;
                    }
            }
        }
        finally
        {
            TempStoragePool.ReturnDocumentIdCollectionList(idsFromGin);
        }
    }

    private void FindExtendedWithFilter(TokenVector searchVector, IMetricsCalculator metricsCalculator,
        CancellationToken cancellationToken)
    {
        var filteredDocuments = TempStoragePool.GetDocumentIdCollectionList<DocumentIdList>();
        var idsFromGin = TempStoragePool.GetDocumentIdCollectionList<DocumentIdList>();
        var sortedList = TempStoragePool.GetDocumentIdCollectionList<DocumentIdList>();

        try
        {
            if (!RelevanceFilter.FindFilteredDocumentsExtendedMerge(GinExtended, searchVector, filteredDocuments,
                    idsFromGin, sortedList))
            {
                return;
            }

            switch (filteredDocuments.Count(vector => vector.Count > 0))
            {
                case 0:
                    {
                        break;
                    }
                case 1:
                    {
                        var idFromGin = filteredDocuments.First(vector => vector.Count > 0);

                        foreach (var documentId in idFromGin)
                        {
                            metricsCalculator.AppendExtended(1, searchVector, documentId, GeneralDirectIndex);
                        }

                        break;
                    }
                default:
                    {
                        if (cancellationToken.IsCancellationRequested)
                            throw new OperationCanceledException(nameof(ExtendedSearchGinMerge));

                        CreateExtendedSearchSpace(searchVector, metricsCalculator, filteredDocuments);

                        break;
                    }
            }
        }
        finally
        {
            TempStoragePool.ReturnDocumentIdCollectionList(sortedList);
            TempStoragePool.ReturnDocumentIdCollectionList(idsFromGin);
            TempStoragePool.ReturnDocumentIdCollectionList(filteredDocuments);
        }
    }

    /// <summary>
    /// Получить список с векторами из GIN на каждый токен вектора поискового запроса.
    /// </summary>
    /// <param name="searchVector">Вектор с поисковым запросом.</param>
    /// <returns>Список векторов GIN.</returns>
    private void CreateExtendedSearchSpace(TokenVector searchVector, IMetricsCalculator metricsCalculator,
        List<DocumentIdList> idsFromGin)
    {
        var list = new List<List<DocumentId>.Enumerator>();

        foreach (var docIdVector in idsFromGin)
        {
            list.Add(docIdVector.GetRawEnumerator());
        }

        List<int> listExists = Enumerable.Range(0, list.Count).ToList();

        for (var index = 0; index < list.Count; index++)
        {
            if (!CollectionsMarshal.AsSpan(list)[index].MoveNext())
            {
                listExists.Remove(index);
            }
        }

        do
        {
            FindMin2(list, listExists, out var minI0, out var docId0, out var docId1);

            START:
            if (docId0.Value < docId1.Value)
            {
                AppendMetric(metricsCalculator, 1, searchVector, docId0);

                ref var enumeratorI = ref CollectionsMarshal.AsSpan(list)[minI0];
                if (!enumeratorI.MoveNext())
                {
                    listExists.Remove(minI0);
                }
                else
                {
                    docId0 = enumeratorI.Current;
                    goto START;
                }
            }
            else if (docId0 == docId1)
            {
                int sIndex = -1;

                for (int i = listExists.Count - 1; i >= 0; i--)
                {
                    int index = listExists[i];

                    ref var enumeratorI = ref CollectionsMarshal.AsSpan(list)[index];
                    if (docId0 == enumeratorI.Current)
                    {
                        sIndex = index;
                        if (!enumeratorI.MoveNext())
                        {
                            listExists.RemoveAt(i);
                        }
                    }
                }

                // поиск в векторе extended
                if (sIndex > -1)
                {
                    var tokensLine = GeneralDirectIndex[docId0];
                    var extendedTokensLine = tokensLine.Extended;

                    var metric = ComputeMetricOrdered(searchVector, extendedTokensLine, sIndex);

                    metricsCalculator.AppendExtended(metric, searchVector, docId0,
                        extendedTokensLine);
                }
            }
        } while (listExists.Count > 1);

        if (listExists.Count > 0)
        {
            int index = listExists[0];
            var enumerator0 = list[index];
            do
            {
                AppendMetric(metricsCalculator, 1, searchVector, enumerator0.Current);
            } while (enumerator0.MoveNext());
        }
    }

    private void AppendMetric(IMetricsCalculator metricsCalculator, int comparisonScore,
        TokenVector searchVector, DocumentId documentId)
    {
        if (EnableRelevanceFilter)
        {
            var tokensLine = GeneralDirectIndex[documentId];
            var reducedTargetVector = tokensLine.Extended;
            int metric = ScoreCalculator.ComputeOrdered(reducedTargetVector, searchVector, 0);

            metricsCalculator.AppendExtended(metric, searchVector, documentId, reducedTargetVector);
        }
        else
        {
            metricsCalculator.AppendExtended(comparisonScore, searchVector, documentId, GeneralDirectIndex);
        }
    }

    private int ComputeMetricOrdered(TokenVector searchVector, TokenVector extendedTokensLine, int sIndex)
    {
        int metric;

        if (EnableRelevanceFilter)
        {
            metric = ScoreCalculator.ComputeOrdered(extendedTokensLine, searchVector, 0);
        }
        else
        {
            metric = ScoreCalculator.ComputeOrdered(extendedTokensLine, searchVector, sIndex);
        }

        return metric;
    }

    private static void FindMin2(List<List<DocumentId>.Enumerator> list, List<int> listExists,
        out int minI0, out DocumentId min0, out DocumentId min1)
    {
        minI0 = listExists[0];
        int minI1 = listExists[1];
        min0 = list[minI0].Current;
        min1 = list[minI1].Current;

        if (min0.Value > min1.Value)
        {
            (minI0, minI1) = (minI1, minI0);
            (min0, min1) = (min1, min0);
        }

        for (int i = 2; i < listExists.Count; i++)
        {
            var index = listExists[i];
            var documentId = list[index].Current;

            if (documentId.Value < min0.Value)
            {
                min1 = min0;
                //minI1 = minI0;
                min0 = documentId;
                minI0 = index;
            }
            else if (documentId.Value < min1.Value)
            {
                min1 = documentId;
                //minI1 = index;
            }
        }
    }
}
