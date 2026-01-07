using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using RD.RsseEngine.Contracts;
using RD.RsseEngine.Dto;
using RD.RsseEngine.Indexes;
using RD.RsseEngine.Iterators;
using RD.RsseEngine.Pools;
using RD.RsseEngine.Processor;
using RD.RsseEngine.SearchType;

namespace RD.RsseEngine.Algorithms;

/// <summary>
/// Класс с алгоритмом подсчёта TfIdf метрики.
/// Пространство поиска формируется с помощью GIN индекса.
/// </summary>
public readonly ref struct Bm25SearchGinArrayDirect
{
    public required TempStoragePool TempStoragePool { private get; init; }

    /// <summary>
    /// Поддержка GIN-индекса.
    /// </summary>
    public required InvertedIndex InvertedIndex { private get; init; }

    public required PositionSearchType PositionSearchType { private get; init; }

    public void FindExtended(TokenVector searchVector, IMetricsCalculator metricsCalculator,
        CancellationToken cancellationToken)
    {
        var idsFromGin = TempStoragePool.InternalDocumentIdListsStorage.Get();

        try
        {
            InvertedIndex.GetDocumentIdVectorsToList(searchVector, idsFromGin);

            if (cancellationToken.IsCancellationRequested)
                throw new OperationCanceledException(nameof(ExtendedSearchGinArrayDirect));

            CreateExtendedSearchSpace(searchVector, metricsCalculator, idsFromGin);
        }
        finally
        {
            TempStoragePool.InternalDocumentIdListsStorage.Return(idsFromGin);
        }
    }

    /// <summary>
    /// Получить список с векторами из GIN на каждый токен вектора поискового запроса.
    /// </summary>
    /// <param name="searchVector">Вектор с поисковым запросом.</param>
    /// <param name="metricsCalculator"></param>
    /// <param name="idsFromGin"></param>
    /// <returns>Список векторов GIN.</returns>
    private void CreateExtendedSearchSpace(TokenVector searchVector, IMetricsCalculator metricsCalculator,
        List<InternalDocumentIdList> idsFromGin)
    {
        var list = TempStoragePool.ListInternalEnumeratorListsStorage.Get();
        var multi = TempStoragePool.IntListsStorage.Get();
        var listExists = TempStoragePool.IntListsStorage.Get();
        var dictionary = TempStoragePool.InternalDocumentIdListCountStorage.Get();

        try
        {
            for (var index = 0; index < idsFromGin.Count; index++)
            {
                var docIdVector = idsFromGin[index];

                list.Add(docIdVector.CreateDocumentListEnumerator());
                multi.Add(0);

                if (dictionary.TryAdd(docIdVector, index))
                {
                    if (CollectionsMarshal.AsSpan(list)[index].MoveNext())
                    {
                        listExists.Add(index);
                    }
                }
                else
                {
                    multi[dictionary[docIdVector]] = 1;
                }
            }

            while (listExists.Count > 1)
            {
                MergeAlgorithm.FindMin(list, listExists, out var minI0, out var docId0, out var docId1);

                var isMulti = multi[minI0] > 0;

            START:
                if (docId0 < docId1)
                {
                    AppendMetric1(isMulti, searchVector, metricsCalculator, docId0, minI0);

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

                    if (sIndex < int.MaxValue)
                    {
                        CalculateAndAppendMetric(metricsCalculator, searchVector, docId0, sIndex);
                    }
                }
            }

            if (listExists.Count == 1)
            {
                AppendMetric2(list, listExists, multi, metricsCalculator, searchVector);
            }
        }
        finally
        {
            TempStoragePool.InternalDocumentIdListCountStorage.Return(dictionary);
            TempStoragePool.IntListsStorage.Return(listExists);
            TempStoragePool.IntListsStorage.Return(multi);
            TempStoragePool.ListInternalEnumeratorListsStorage.Return(list);
        }
    }

    private void AppendMetric1(bool isMulti, TokenVector searchVector, IMetricsCalculator metricsCalculator,
        InternalDocumentId documentId, int minI0)
    {

        if (isMulti)
        {
            CalculateAndAppendMetric(metricsCalculator, searchVector, documentId, minI0);
        }
        else
        {
            if (InvertedIndex.TryGetOffsetTokenVector(documentId, out var offsetTokenVector, out var externalDocument))
            {
                var bm25Calculator = InvertedIndex.CreateBm25Calculator();
                var metric = bm25Calculator.CalculateBm25(searchVector, externalDocument.Size, offsetTokenVector);

                //metricsCalculator.AppendExtendedMetric(metric, searchVector, externalDocument);
            }
        }
    }

    private void AppendMetric2(List<InternalDocumentListEnumerator> list, List<int> listExists, List<int> multi,
        IMetricsCalculator metricsCalculator, TokenVector searchVector)
    {
        var index = listExists[0];
        var enumerator = list[index];

        if (multi[index] > 0)
        {
            do
            {
                var documentId = enumerator.Current;
                CalculateAndAppendMetric(metricsCalculator, searchVector, documentId, index);
            } while (enumerator.MoveNext());
        }
        else
        {
            do
            {
                var documentId = enumerator.Current;
                if (InvertedIndex.TryGetOffsetTokenVector(documentId, out _, out var externalDocument))
                {
                    const int metric = 1;
                    metricsCalculator.AppendExtendedMetric(metric, searchVector, externalDocument);
                }
            } while (enumerator.MoveNext());
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void CalculateAndAppendMetric(IMetricsCalculator metricsCalculator, TokenVector searchVector,
        InternalDocumentId documentId, int sIndex)
    {
        if (!InvertedIndex.TryGetOffsetTokenVector(documentId, out var offsetTokenVector, out var externalDocument))
        {
            return;
        }

        switch (PositionSearchType)
        {
            case PositionSearchType.LinearScan:
                {
                    var position = -1;
                    var metric = 0;

                    for (var i = sIndex; i < searchVector.Count; i++)
                    {
                        var token = searchVector.ElementAt(i);

                        if (offsetTokenVector.TryFindNextTokenPositionLinearScan(token, ref position))
                        {
                            metric++;
                        }
                    }

                    metricsCalculator.AppendExtendedMetric(metric, searchVector, externalDocument);

                    break;
                }
            case PositionSearchType.BinarySearch:
                {
                    var position = -1;
                    var metric = 0;

                    for (var i = sIndex; i < searchVector.Count; i++)
                    {
                        var token = searchVector.ElementAt(i);

                        if (offsetTokenVector.TryFindNextTokenPositionBinarySearch(token, ref position))
                        {
                            metric++;
                        }
                    }

                    metricsCalculator.AppendExtendedMetric(metric, searchVector, externalDocument);

                    break;
                }
            default:
                {
                    throw new NotSupportedException($"PositionSearchType {PositionSearchType} not supported.");
                }
        }
    }

    private static void SwapAndRemoveAt(List<int> listExists, int i)
    {
        listExists[i] = listExists[listExists.Count - 1];
        listExists.RemoveAt(listExists.Count - 1);
    }
}
