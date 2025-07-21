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
public sealed class ExtendedSearchGinMergeFilter : IExtendedSearchProcessor
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
        var filteredDocuments = TempStoragePool.GetDocumentIdCollectionList<DocumentIdList>();
        var idsFromGin = TempStoragePool.GetDocumentIdCollectionList<DocumentIdList>();
        var sortedIds = TempStoragePool.GetDocumentIdCollectionList<DocumentIdList>();

        try
        {
            if (!RelevanceFilter.FindFilteredDocumentsExtendedMerge(GinExtended, searchVector, idsFromGin,
                    sortedIds, out var filteredTokensCount))
            {
                return;
            }

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

                        CreateExtendedSearchSpace(searchVector, metricsCalculator, sortedIds, filteredTokensCount);

                        break;
                    }
            }
        }
        finally
        {
            TempStoragePool.ReturnDocumentIdCollectionList(sortedIds);
            TempStoragePool.ReturnDocumentIdCollectionList(idsFromGin);
            TempStoragePool.ReturnDocumentIdCollectionList(filteredDocuments);
        }
    }

    /// <summary>
    /// Получить список с векторами из GIN на каждый токен вектора поискового запроса.
    /// </summary>
    /// <param name="searchVector">Вектор с поисковым запросом.</param>
    /// <param name="metricsCalculator"></param>
    /// <param name="sortedIds"></param>
    /// <param name="filteredTokensCount"></param>
    /// <returns>Список векторов GIN.</returns>
    private void CreateExtendedSearchSpace(TokenVector searchVector, IMetricsCalculator metricsCalculator,
        List<DocumentIdList> sortedIds, int filteredTokensCount)
    {
        var list = TempStoragePool.ListEnumeratorListsStorage.Get();
        var listExists = TempStoragePool.IntListsStorage.Get();
        var dictionary = TempStoragePool.DocumentIdListCountStorage.Get();

        try
        {
            for (var index = 0; index < filteredTokensCount; index++)
            {
                var docIdVector = sortedIds[index];

                list.Add(docIdVector.CreateDocumentListEnumerator());

                if (dictionary.TryAdd(docIdVector, index))
                {
                    if (CollectionsMarshal.AsSpan(list)[index].MoveNext())
                    {
                        listExists.Add(index);
                    }
                }
            }

            if (listExists.Count == 1)
            {
                AppendMetric2(list, listExists, metricsCalculator, searchVector);
                return;
            }

            do
            {
                MergeAlgorithm.FindMin(list, listExists, out var minI0, out var docId0, out var docId1);

            START:
                if (docId0.Value < docId1.Value)
                {
                    AppendMetric1(searchVector, metricsCalculator, docId0);

                    ref var enumeratorI = ref CollectionsMarshal.AsSpan(list)[minI0];
                    if (!enumeratorI.MoveNext())
                    {
                        var i = listExists.IndexOf(minI0);
                        SwapAndRemoveAt(listExists, i);
                    }
                    else
                    {
                        docId0 = enumeratorI.Current;
                        goto START;
                    }
                }
                else if (docId0 == docId1)
                {
                    var sIndex = int.MaxValue;

                    for (var i = listExists.Count - 1; i >= 0; i--)
                    {
                        var index = listExists[i];

                        ref var enumeratorI = ref CollectionsMarshal.AsSpan(list)[index];
                        if (docId0 == enumeratorI.Current)
                        {
                            sIndex = Math.Min(sIndex, index);
                            if (!enumeratorI.MoveNext())
                            {
                                SwapAndRemoveAt(listExists, i);
                            }
                        }
                    }

                    // поиск в векторе extended
                    if (sIndex < int.MaxValue)
                    {
                        CalculateAndAppendMetric(metricsCalculator, searchVector, docId0);
                    }
                }
            } while (listExists.Count > 1);

            if (listExists.Count == 1)
            {
                AppendMetric2(list, listExists, metricsCalculator, searchVector);
            }
        }
        finally
        {
            TempStoragePool.DocumentIdListCountStorage.Return(dictionary);
            TempStoragePool.IntListsStorage.Return(listExists);
            TempStoragePool.ListEnumeratorListsStorage.Return(list);
        }
    }

    private void AppendMetric1(TokenVector searchVector, IMetricsCalculator metricsCalculator,
        DocumentId documentId)
    {
        CalculateAndAppendMetric(metricsCalculator, searchVector, documentId);
    }

    private void AppendMetric2(List<DocumentListEnumerator> list, List<int> listExists,
        IMetricsCalculator metricsCalculator, TokenVector searchVector)
    {
        var index = listExists[0];
        var enumerator = list[index];

        do
        {
            var documentId = enumerator.Current;
            CalculateAndAppendMetric(metricsCalculator, searchVector, documentId);
        } while (enumerator.MoveNext());
    }

    private void CalculateAndAppendMetric(IMetricsCalculator metricsCalculator, TokenVector searchVector,
        DocumentId documentId)
    {
        // поиск в векторе extended
        var extendedTokensLine = GeneralDirectIndex[documentId].Extended;
        var metric = ScoreCalculator.ComputeOrdered(extendedTokensLine, searchVector);
        metricsCalculator.AppendExtended(metric, searchVector, documentId, extendedTokensLine);
    }

    private static void SwapAndRemoveAt(List<int> listExists, int i)
    {
        listExists[i] = listExists[listExists.Count - 1];
        listExists.RemoveAt(listExists.Count - 1);
    }
}
