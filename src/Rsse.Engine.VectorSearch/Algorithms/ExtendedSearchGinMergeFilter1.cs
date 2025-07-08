using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using RsseEngine.Contracts;
using RsseEngine.Dto;
using RsseEngine.Indexes;
using RsseEngine.Iterators;
using RsseEngine.Pools;
using RsseEngine.Processor;

namespace RsseEngine.Algorithms;

/// <summary>
/// Класс с алгоритмом подсчёта расширенной метрики.
/// Пространство поиска формируется с помощью GIN индекса, применены дополнительные оптимизации.
/// </summary>
public sealed class ExtendedSearchGinMergeFilter1 : IExtendedSearchProcessor
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

    /// <inheritdoc/>
    public void FindExtended(TokenVector searchVector, IMetricsCalculator metricsCalculator,
        CancellationToken cancellationToken)
    {
        //var filteredDocuments = TempStoragePool.GetDocumentIdCollectionList<DocumentIdList>();
        var filteredDocuments = new List<GinRelevanceFilter.CollectionWithCost<DocumentIdList>>();
        var idsFromGin = TempStoragePool.GetDocumentIdCollectionList<DocumentIdList>();
        //var sortedList = TempStoragePool.GetDocumentIdCollectionList<DocumentIdList>();
        var sortedList = new List<GinRelevanceFilter.CollectionWithCost<DocumentIdList>>();

        try
        {
            if (!RelevanceFilter.FindFilteredDocumentsExtendedMergeEx(GinExtended, searchVector, idsFromGin,
                    filteredDocuments, sortedList))
            {
                return;
            }

            switch (sortedList.Count(vector => vector.Collection.Count > 0))
            {
                case 0:
                    {
                        break;
                    }
                case 1:
                    {
                        GinRelevanceFilter.CollectionWithCost<DocumentIdList> idFromGin = sortedList.First(vector => vector.Collection.Count > 0);

                        if (idFromGin.Cost > 1)
                        {
                            foreach (var documentId in idFromGin.Collection)
                            {
                                var extendedTokensLine = GeneralDirectIndex[documentId].Extended;

                                int sIndex = 0;
                                var metric = ComputeMetricOrdered(searchVector, extendedTokensLine, sIndex);

                                metricsCalculator.AppendExtended(metric, searchVector, documentId, extendedTokensLine.Count);
                            }
                        }
                        else
                        {
                            foreach (var documentId in idFromGin.Collection)
                            {
                                metricsCalculator.AppendExtended(idFromGin.Cost, searchVector, documentId, GeneralDirectIndex);
                            }
                        }

                        break;
                    }
                default:
                    {
                        if (cancellationToken.IsCancellationRequested)
                            throw new OperationCanceledException(nameof(ExtendedSearchGinMergeFilter1));

                        CreateExtendedSearchSpace(searchVector, metricsCalculator, sortedList);

                        break;
                    }
            }
        }
        finally
        {
            //TempStoragePool.ReturnDocumentIdCollectionList(sortedList);
            TempStoragePool.ReturnDocumentIdCollectionList(idsFromGin);
            //TempStoragePool.ReturnDocumentIdCollectionList(filteredDocuments);
        }
    }

    /// <summary>
    /// Получить список с векторами из GIN на каждый токен вектора поискового запроса.
    /// </summary>
    /// <param name="searchVector">Вектор с поисковым запросом.</param>
    /// <returns>Список векторов GIN.</returns>
    private void CreateExtendedSearchSpace(TokenVector searchVector, IMetricsCalculator metricsCalculator,
        List<GinRelevanceFilter.CollectionWithCost<DocumentIdList>> idsFromGin)
    {
        var list = new List<DocumentListEnumerator>();

        foreach (var docIdVector in idsFromGin)
        {
            list.Add(docIdVector.Collection.CreateDocumentListEnumerator());
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
            MergeAlgorithm.FindMin(list, listExists, out var minI0, out var docId0, out var docId1);

            START:
            if (docId0.Value < docId1.Value)
            {
                AppendMetric(metricsCalculator, idsFromGin[minI0].Cost, searchVector, docId0);

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
                        extendedTokensLine.Count);
                }
            }
        } while (listExists.Count > 1);

        if (listExists.Count > 0)
        {
            int index = listExists[0];
            var enumerator0 = list[index];
            do
            {
                AppendMetric(metricsCalculator, idsFromGin[index].Cost, searchVector, enumerator0.Current);
            } while (enumerator0.MoveNext());
        }
    }

    private void AppendMetric(IMetricsCalculator metricsCalculator, int comparisonScore,
        TokenVector searchVector, DocumentId documentId)
    {
        //metricsCalculator.AppendExtended(comparisonScore, searchVector, documentId, GeneralDirectIndex);

        var tokensLine = GeneralDirectIndex[documentId];
        var reducedTargetVector = tokensLine.Extended;
        int metric = ScoreCalculator.ComputeOrdered(reducedTargetVector, searchVector, 0);

        metricsCalculator.AppendExtended(metric, searchVector, documentId, reducedTargetVector.Count);
    }

    private int ComputeMetricOrdered(TokenVector searchVector, TokenVector extendedTokensLine, int sIndex)
    {
        return ScoreCalculator.ComputeOrdered(extendedTokensLine, searchVector, 0);
    }
}
