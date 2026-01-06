using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using RsseEngine.Contracts;
using RsseEngine.Dto.Common;
using RsseEngine.Dto.Offsets;
using RsseEngine.Indexes;
using RsseEngine.Iterators;
using RsseEngine.Pools;
using RsseEngine.Processor;

namespace RsseEngine.Algorithms;

/// <summary>
/// Класс с алгоритмом подсчёта расширенной метрики.
/// Пространство поиска формируется с помощью GIN индекса, применены дополнительные оптимизации.
/// </summary>
public readonly ref struct ExtendedSearchGinOffsetFilter : IExtendedSearchProcessor
{
    public required TempStoragePool TempStoragePool { private get; init; }

    /// <summary>
    /// Общий инвертированный индекс: токен-идентификаторы.
    /// </summary>
    public required InvertedOffsetIndex GinExtended { private get; init; }

    public required RelevanceFilter RelevanceFilter { private get; init; }

    /// <inheritdoc/>
    public void FindExtended(TokenVector searchVector, IMetricsCalculator metricsCalculator,
        CancellationToken cancellationToken)
    {
        var enumerators = TempStoragePool.OffsetEnumeratorCollections.Get();

        try
        {
            if (!RelevanceFilter.TryGetRelevantDocumentsEnumerators(GinExtended, searchVector, enumerators,
                    out var indexWithCounts, out var filteredTokensCount,
                    out var minRelevancyCount))
            {
                return;
            }

            if (cancellationToken.IsCancellationRequested)
                throw new OperationCanceledException(nameof(ExtendedSearchGinOffsetFilter));

            switch (enumerators.Count)
            {
                case 0:
                    {
                        break;
                    }
                case 1:
                    {
                        ProcessSingle(searchVector, metricsCalculator, enumerators);
                        break;
                    }
                default:
                    {
                        /*new SearchProcessor
                        {
                            GinExtended = GinExtended
                        }.ProcessMulti(searchVector, metricsCalculator, enumerators, indexWithCounts,
                            filteredTokensCount, minRelevancyCount);*/

                        new SearchProcessorWithIndexesDeduplication
                        {
                            GinExtended = GinExtended
                        }.ProcessMulti(searchVector, metricsCalculator, enumerators, indexWithCounts,
                            filteredTokensCount, minRelevancyCount);

                        break;
                    }
            }
        }
        finally
        {
            TempStoragePool.OffsetEnumeratorCollections.Return(enumerators);
        }
    }

    private void ProcessSingle(TokenVector searchVector, IMetricsCalculator metricsCalculator,
        List<DocumentIdsExtendedEnumerator> enumerators)
    {
        var enumerator = enumerators[0];

        do
        {
            var documentId = enumerator.Current;
            const int metric = 1;

            if (GinExtended.TryGetExternalDocumentId(documentId, out var externalDocument))
            {
                metricsCalculator.AppendExtendedMetric(metric, searchVector, externalDocument);
            }

        } while (enumerator.MoveNext());
    }

    private static void CreateIndexWithLastList(List<DocumentIdsExtendedEnumerator> enumerators,
        List<IndexWithLast> exList, Dictionary<List<InternalDocumentId>, int> dictionary, HashSet<int> set)
    {
        for (var i = 0; i < enumerators.Count; i++)
        {
            var enumerator = enumerators[i];

            ref var enumeratorIndex = ref CollectionsMarshal.GetValueRefOrAddDefault(
                dictionary, enumerator.InternalDocumentIds, out var exists);

            if (!exists)
            {
                enumeratorIndex = i;
            }

            exList.Add(new(index: enumeratorIndex, last: false));
        }

        for (var i = exList.Count - 1; i >= 0; i--)
        {
            var enumeratorIndex = exList[i].Index;

            if (set.Add(enumeratorIndex))
            {
                exList[i] = new(index: enumeratorIndex, last: true);
            }
        }
    }

    private readonly struct SearchProcessor
    {
        public required InvertedOffsetIndex GinExtended { private get; init; }

        public void ProcessMulti(TokenVector searchVector, IMetricsCalculator metricsCalculator,
            List<DocumentIdsExtendedEnumerator> enumerators, List<IndexWithCount> indexWithCounts,
            int filteredTokensCount, int minRelevancyCount)
        {
            var docIdIterators = PrepareDocumentIdsWithOffsetsAndEnumerators(
                enumerators, indexWithCounts, filteredTokensCount, out var exList);

            while (docIdIterators.Count > 0)
            {
                FindMin(docIdIterators, out var documentIdIndex, out var documentId,
                    out var documentIdNext, out var min1Exists);

                if (min1Exists && documentId < documentIdNext || !min1Exists)
                {
                    ref var enumerator = ref CollectionsMarshal.AsSpan(docIdIterators)[documentIdIndex];

                    if (!enumerator.MoveNext())
                    {
                        docIdIterators.RemoveAt(documentIdIndex);
                    }
                }
                else
                {
                    for (var i = 0; i < docIdIterators.Count; i++)
                    {
                        ref var enumerator = ref CollectionsMarshal.AsSpan(docIdIterators)[i];

                        if (enumerator.Current == documentId)
                        {
                            if (!enumerator.MoveNext())
                            {
                                docIdIterators.RemoveAt(i);
                                i--;
                            }
                        }
                    }
                }

                var position = -1;
                var metric = 0;

                for (var i = 0; i < exList.Count; i++)
                {
                    if (metric + exList.Count - i < minRelevancyCount)
                    {
                        break;
                    }

                    var exIndex = exList[i];
                    ref var enumerator = ref CollectionsMarshal.AsSpan(enumerators)[exIndex.Index];

                    var documentId1 = enumerator.Current;

                    if (documentId1 < documentId)
                    {
                        if (!enumerator.MoveNextBinarySearch(documentId))
                        {
                            exList.RemoveAt(i);
                            i--;

                            continue;
                        }

                        documentId1 = enumerator.Current;
                    }

                    if (documentId1 == documentId)
                    {
                        if (enumerator.TryFindNextPosition(ref position))
                        {
                            metric++;
                        }

                        var indexWithLast = exList[i];

                        if (indexWithLast.Last)
                        {
                            if (!enumerator.MoveNext())
                            {
                                for (var j = exList.Count - 1; j >= 0; j--)
                                {
                                    if (exList[j].Index == indexWithLast.Index)
                                    {
                                        exList.RemoveAt(j);
                                        i--;
                                    }
                                }
                            }
                        }
                    }
                }

                if (position >= 0)
                {
                    if (GinExtended.TryGetExternalDocumentId(documentId, out var externalDocument))
                    {
                        metricsCalculator.AppendExtendedMetric(metric, searchVector, externalDocument);
                    }
                }

                if (exList.Count < minRelevancyCount)
                {
                    return;
                }
            }
        }

        private static List<DocumentIdsExtendedEnumerator> PrepareDocumentIdsWithOffsetsAndEnumerators(
            List<DocumentIdsExtendedEnumerator> enumerators, List<IndexWithCount> indexWithCounts,
            int filteredTokensCount, out List<IndexWithLast> exList)
        {
            var dictionary = new Dictionary<List<InternalDocumentId>, int>();
            var set = new HashSet<int>();

            exList = new List<IndexWithLast>();

            CreateIndexWithLastList(enumerators, exList, dictionary, set);

            var documentIdIterators = new List<DocumentIdsExtendedEnumerator>();

            for (var i = 0; i < filteredTokensCount; i++)
            {
                var indexWithCount = indexWithCounts[i];
                var enumerator = enumerators[indexWithCount.Index];

                documentIdIterators.Add(enumerator);
            }

            return documentIdIterators;
        }

        private static void FindMin(List<DocumentIdsExtendedEnumerator> list,
            out int minI0, out InternalDocumentId min0, out InternalDocumentId min1, out bool min1Exists)
        {
            if (list.Count > 1)
            {
                MergeHelpers.FindTwoMinimumIds(list, out minI0, out min0, out min1);
                min1Exists = true;
            }
            else
            {
                minI0 = 0;
                min0 = list[minI0].Current;
                min1 = default;
                min1Exists = false;
            }
        }
    }

    private readonly struct SearchProcessorWithIndexesDeduplication
    {
        public required InvertedOffsetIndex GinExtended { private get; init; }

        public void ProcessMulti(TokenVector searchVector, IMetricsCalculator metricsCalculator,
            List<DocumentIdsExtendedEnumerator> enumerators, List<IndexWithCount> indexWithCounts,
            int filteredTokensCount, int minRelevancyCount)
        {
            var docIdIterators = PrepareDocumentIdsWithOffsetsAndEnumerators(
                enumerators, indexWithCounts, filteredTokensCount, out var exList);

            while (docIdIterators.Count > 0)
            {
                FindMin(enumerators, docIdIterators, out var documentIdIndex, out var documentId,
                    out var documentIdNext, out var min1Exists);

                var position = -1;
                var metric = 0;

                for (var i = 0; i < exList.Count; i++)
                {
                    if (metric + exList.Count - i < minRelevancyCount && documentId != documentIdNext && min1Exists)
                    {
                        ref var enumerator2 = ref CollectionsMarshal.AsSpan(enumerators)[documentIdIndex];

                        if (enumerator2.Current == documentId)
                        {
                            if (!enumerator2.MoveNext())
                            {
                                docIdIterators.Remove(documentIdIndex);

                                for (var j = exList.Count - 1; j >= 0; j--)
                                {
                                    if (exList[j].Index == documentIdIndex)
                                    {
                                        exList.RemoveAt(j);
                                    }
                                }
                            }
                        }

                        break;
                    }

                    var exIndex = exList[i];
                    ref var enumerator = ref CollectionsMarshal.AsSpan(enumerators)[exIndex.Index];

                    var documentId1 = enumerator.Current;

                    if (documentId1 < documentId)
                    {
                        if (!enumerator.MoveNextBinarySearch(documentId))
                        {
                            docIdIterators.Remove(exList[i].Index);
                            exList.RemoveAt(i);
                            i--;

                            continue;
                        }

                        documentId1 = enumerator.Current;
                    }

                    if (documentId1 == documentId)
                    {
                        if (enumerator.TryFindNextPosition(ref position))
                        {
                            metric++;
                        }

                        var indexWithLast = exList[i];

                        if (indexWithLast.Last)
                        {
                            if (!enumerator.MoveNext())
                            {
                                for (var j = exList.Count - 1; j >= 0; j--)
                                {
                                    if (exList[j].Index == indexWithLast.Index)
                                    {
                                        docIdIterators.Remove(exList[j].Index);
                                        exList.RemoveAt(j);
                                        i--;
                                    }
                                }
                            }
                        }
                    }
                }

                if (position >= 0)
                {
                    if (GinExtended.TryGetExternalDocumentId(documentId, out var externalDocument))
                    {
                        metricsCalculator.AppendExtendedMetric(metric, searchVector, externalDocument);
                    }
                }

                if (exList.Count < minRelevancyCount)
                {
                    return;
                }
            }
        }

        private static List<int> PrepareDocumentIdsWithOffsetsAndEnumerators(
            List<DocumentIdsExtendedEnumerator> enumerators, List<IndexWithCount> indexWithCounts,
            int filteredTokensCount, out List<IndexWithLast> exList)
        {
            var dictionary = new Dictionary<List<InternalDocumentId>, int>();
            var set = new HashSet<int>();

            exList = new List<IndexWithLast>();

            CreateIndexWithLastList(enumerators, exList, dictionary, set);

            set.Clear();

            var documentIdIterators = new List<int>();

            for (var i = 0; i < filteredTokensCount; i++)
            {
                var indexWithCount = indexWithCounts[i];
                var indexWithLast = exList[indexWithCount.Index];
                var enumeratorIndex = indexWithLast.Index;

                if (set.Add(enumeratorIndex))
                {
                    documentIdIterators.Add(enumeratorIndex);
                }
            }

            return documentIdIterators;
        }

        private static void FindMin(List<DocumentIdsExtendedEnumerator> list, List<int> listExists,
            out int minI0, out InternalDocumentId min0, out InternalDocumentId min1, out bool min1Exists)
        {
            if (listExists.Count > 1)
            {
                MergeHelpers.FindTwoMinimumIdsFromSubset(list, listExists, out minI0, out min0, out min1);
                min1Exists = true;
            }
            else
            {
                minI0 = listExists[0];
                min0 = list[minI0].Current;
                min1 = default;
                min1Exists = false;
            }
        }
    }
}
