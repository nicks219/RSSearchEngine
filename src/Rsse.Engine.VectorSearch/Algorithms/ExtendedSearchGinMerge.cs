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

    /// <inheritdoc/>
    public void FindExtended(TokenVector searchVector, IMetricsCalculator metricsCalculator,
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

    /// <summary>
    /// Получить список с векторами из GIN на каждый токен вектора поискового запроса.
    /// </summary>
    /// <param name="searchVector">Вектор с поисковым запросом.</param>
    /// <param name="metricsCalculator"></param>
    /// <param name="idsFromGin"></param>
    /// <returns>Список векторов GIN.</returns>
    private void CreateExtendedSearchSpace(TokenVector searchVector, IMetricsCalculator metricsCalculator,
        List<DocumentIdList> idsFromGin)
    {
        var list = TempStoragePool.ListEnumeratorListsStorage.Get();
        var multi = TempStoragePool.IntListsStorage.Get();
        var listExists = TempStoragePool.IntListsStorage.Get();
        var dictionary = TempStoragePool.DocumentIdListCountStorage.Get();

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

            if (listExists.Count == 1)
            {
                AppendMetric2(list, listExists, multi, metricsCalculator, searchVector);
                return;
            }

            do
            {
                MergeAlgorithm.FindMin(list, listExists, out var minI0, out var docId0, out var docId1);

                var isMulti = multi[minI0] > 0;

                START:
                if (docId0.Value < docId1.Value)
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
                        metricsCalculator.AppendExtendedMetric(searchVector, docId0, GeneralDirectIndex, sIndex);
                    }
                }
            } while (listExists.Count > 1);

            if (listExists.Count == 1)
            {
                AppendMetric2(list, listExists, multi, metricsCalculator, searchVector);
            }
        }
        finally
        {
            TempStoragePool.DocumentIdListCountStorage.Return(dictionary);
            TempStoragePool.IntListsStorage.Return(listExists);
            TempStoragePool.IntListsStorage.Return(multi);
            TempStoragePool.ListEnumeratorListsStorage.Return(list);
        }
    }

    private void AppendMetric1(bool isMulti, TokenVector searchVector, IMetricsCalculator metricsCalculator,
        DocumentId documentId, int minI0)
    {
        if (isMulti)
        {
            metricsCalculator.AppendExtendedMetric(searchVector, documentId, GeneralDirectIndex, minI0);
        }
        else
        {
            const int metric = 1;
            metricsCalculator.AppendExtended(metric, searchVector, documentId, GeneralDirectIndex);
        }
    }

    private void AppendMetric2(List<DocumentListEnumerator> list, List<int> listExists, List<int> multi,
        IMetricsCalculator metricsCalculator, TokenVector searchVector)
    {
        var index = listExists[0];
        var enumerator = list[index];

        if (multi[index] > 0)
        {
            do
            {
                var documentId = enumerator.Current;
                metricsCalculator.AppendExtendedMetric(searchVector, documentId, GeneralDirectIndex, index);
            } while (enumerator.MoveNext());
        }
        else
        {
            do
            {
                var documentId = enumerator.Current;
                const int metric = 1;
                metricsCalculator.AppendExtended(metric, searchVector, documentId, GeneralDirectIndex);
            } while (enumerator.MoveNext());
        }
    }

    private static void SwapAndRemoveAt(List<int> listExists, int i)
    {
        listExists[i] = listExists[listExists.Count - 1];
        listExists.RemoveAt(listExists.Count - 1);
    }
}
