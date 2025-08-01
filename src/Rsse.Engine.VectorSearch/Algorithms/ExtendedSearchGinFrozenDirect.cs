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
public sealed class ExtendedSearchGinFrozenDirect : IExtendedSearchProcessor
{
    public required TempStoragePool TempStoragePool { private get; init; }

    /// <summary>
    /// Индекс для всех токенизированных заметок.
    /// </summary>
    public required DirectIndex GeneralDirectIndex { get; init; }

    /// <summary>
    /// Поддержка GIN-индекса.
    /// </summary>
    public required FrozenDirectOffsetIndex GinExtended { get; init; }

    /// <inheritdoc/>
    public void FindExtended(TokenVector searchVector, IMetricsCalculator metricsCalculator,
        CancellationToken cancellationToken)
    {
        var idsFromGin = TempStoragePool.InternalDocumentIdListsStorage.Get();

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
                            if (GinExtended.TryGetOffsetTokenVector(documentId, out _, out var externalDocumentId))
                            {
                                const int metric = 1;
                                metricsCalculator.AppendExtended(metric, searchVector, externalDocumentId, GeneralDirectIndex);
                            }
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
            if (GinExtended.TryGetOffsetTokenVector(documentId, out _, out var externalDocumentId))
            {
                const int metric = 1;
                metricsCalculator.AppendExtended(metric, searchVector, externalDocumentId, GeneralDirectIndex);
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
                if (GinExtended.TryGetOffsetTokenVector(documentId, out _, out var externalDocumentId))
                {
                    const int metric = 1;
                    metricsCalculator.AppendExtended(metric, searchVector, externalDocumentId, GeneralDirectIndex);
                }
            } while (enumerator.MoveNext());
        }
    }

    private void CalculateAndAppendMetric(IMetricsCalculator metricsCalculator, TokenVector searchVector,
        InternalDocumentId documentId, int sIndex)
    {
        if (!GinExtended.TryGetOffsetTokenVector(documentId, out var offsetTokenVector, out var externalDocumentId))
        {
            return;
        }

        var position = -1;
        var metric = 0;

        for (var i = sIndex; i < searchVector.Count; i++)
        {
            var token = searchVector.ElementAt(i);

            if (offsetTokenVector.TryFindNextTokenPosition(token, ref position))
            {
                metric++;
            }
        }

        metricsCalculator.AppendExtended(metric, searchVector, externalDocumentId, GeneralDirectIndex);
    }

    private static void SwapAndRemoveAt(List<int> listExists, int i)
    {
        listExists[i] = listExists[listExists.Count - 1];
        listExists.RemoveAt(listExists.Count - 1);
    }
}
