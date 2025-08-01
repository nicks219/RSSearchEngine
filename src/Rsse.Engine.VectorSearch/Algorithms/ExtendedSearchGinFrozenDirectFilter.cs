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
public sealed class ExtendedSearchGinFrozenDirectFilter : IExtendedSearchProcessor
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

    public required GinRelevanceFilter RelevanceFilter { private get; init; }

    /// <inheritdoc/>
    public void FindExtended(TokenVector searchVector, IMetricsCalculator metricsCalculator,
        CancellationToken cancellationToken)
    {
        var idsFromGin = TempStoragePool.InternalDocumentIdListsStorage.Get();
        var sortedIds = TempStoragePool.InternalDocumentIdListsStorage.Get();

        try
        {
            if (!RelevanceFilter.FindFilteredDocumentsExtendedMerge(GinExtended, searchVector, idsFromGin,
                    sortedIds, out var filteredTokensCount, out var minRelevancyCount))
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
                            if (GinExtended.TryGetOffsetTokenVector(documentId, out _, out var externalDocumentId))
                            {
                                metricsCalculator.AppendExtended(1, searchVector, externalDocumentId, GeneralDirectIndex);
                            }
                        }

                        break;
                    }
                default:
                    {
                        if (cancellationToken.IsCancellationRequested)
                            throw new OperationCanceledException(nameof(ExtendedSearchGinMerge));

                        CreateExtendedSearchSpace(searchVector, metricsCalculator, sortedIds, filteredTokensCount, minRelevancyCount);

                        break;
                    }
            }
        }
        finally
        {
            TempStoragePool.InternalDocumentIdListsStorage.Return(sortedIds);
            TempStoragePool.InternalDocumentIdListsStorage.Return(idsFromGin);
        }
    }

    /// <summary>
    /// Получить список с векторами из GIN на каждый токен вектора поискового запроса.
    /// </summary>
    /// <param name="searchVector">Вектор с поисковым запросом.</param>
    /// <param name="metricsCalculator"></param>
    /// <param name="sortedIds"></param>
    /// <param name="filteredTokensCount"></param>
    /// <param name="minRelevancyCount">Количество векторов обеспечивающих релевантность.</param>
    /// <returns>Список векторов GIN.</returns>
    private void CreateExtendedSearchSpace(TokenVector searchVector, IMetricsCalculator metricsCalculator,
        List<InternalDocumentIdList> sortedIds, int filteredTokensCount, int minRelevancyCount)
    {
        var list = TempStoragePool.ListInternalEnumeratorListsStorage.Get();
        var listExists = TempStoragePool.IntListsStorage.Get();
        var dictionary = TempStoragePool.InternalDocumentIdListCountStorage.Get();

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

            while (listExists.Count > 1)
            {
                MergeAlgorithm.FindMin(list, listExists, out var minI0, out var docId0, out var docId1);

            START:
                if (docId0.Value < docId1.Value)
                {
                    CalculateAndAppendMetric(metricsCalculator, searchVector, docId0, minRelevancyCount);

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
                    for (var i = listExists.Count - 1; i >= 0; i--)
                    {
                        var index = listExists[i];

                        ref var enumeratorI = ref CollectionsMarshal.AsSpan(list)[index];
                        if (docId0 == enumeratorI.Current)
                        {
                            if (!enumeratorI.MoveNext())
                            {
                                SwapAndRemoveAt(listExists, i);
                            }
                        }
                    }

                    CalculateAndAppendMetric(metricsCalculator, searchVector, docId0, minRelevancyCount);
                }
            }

            if (listExists.Count == 1)
            {
                var index = listExists[0];
                var enumerator = list[index];

                do
                {
                    var documentId = enumerator.Current;
                    CalculateAndAppendMetric(metricsCalculator, searchVector, documentId, minRelevancyCount);
                } while (enumerator.MoveNext());
            }
        }
        finally
        {
            TempStoragePool.InternalDocumentIdListCountStorage.Return(dictionary);
            TempStoragePool.IntListsStorage.Return(listExists);
            TempStoragePool.ListInternalEnumeratorListsStorage.Return(list);
        }
    }

    private void CalculateAndAppendMetric(IMetricsCalculator metricsCalculator, TokenVector searchVector,
        InternalDocumentId documentId, int minRelevancyCount)
    {
        if (!GinExtended.TryGetOffsetTokenVector(documentId, out var offsetTokenVector, out var externalDocumentId))
        {
            return;
        }

        var position = -1;
        var empty = 0;

        for (var i = 0; i < searchVector.Count; i++)
        {
            var token = searchVector.ElementAt(i);

            if (!offsetTokenVector.TryFindNextTokenPosition(token, ref position))
            {
                empty++;

                if (empty > searchVector.Count - minRelevancyCount)
                {
                    return;
                }
            }
        }

        var metric = searchVector.Count - empty;

        metricsCalculator.AppendExtended(metric, searchVector, externalDocumentId, GeneralDirectIndex);
    }

    private static void SwapAndRemoveAt(List<int> listExists, int i)
    {
        listExists[i] = listExists[listExists.Count - 1];
        listExists.RemoveAt(listExists.Count - 1);
    }
}
