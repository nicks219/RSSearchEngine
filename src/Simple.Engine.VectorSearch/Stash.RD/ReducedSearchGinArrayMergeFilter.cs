# if IS_RD_PROJECT

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using SimpleEngine.Contracts;
using SimpleEngine.Dto.Common;
using SimpleEngine.Indexes;
using SimpleEngine.Iterators;
using SimpleEngine.Pools;
using SimpleEngine.Processor;

namespace SimpleEngine.Algorithms;

/// <summary>
/// Класс с алгоритмом подсчёта сокращенной метрики.
/// Метрика считается GIN индексе, применены дополнительные оптимизации.
/// </summary>
public readonly ref struct ReducedSearchGinArrayMergeFilter : IReducedSearchProcessor
{
    public required TempStoragePool TempStoragePool { private get; init; }

    /// <summary>
    /// Поддержка GIN-индекса.
    /// </summary>
    public required CommonIndex CommonIndex { private get; init; }

    public required RelevanceFilter RelevanceFilter { private get; init; }

    /// <inheritdoc/>
    public void FindReduced(TokenVector searchVector, IMetricsCalculator metricsCalculator,
        CancellationToken cancellationToken)
    {
        var sortedIds = TempStoragePool.InternalIdsWithTokenCollections.Get();

        try
        {
            if (!RelevanceFilter.TryGetRelevantDocumentsForReducedSearch(CommonIndex, searchVector,
                    sortedIds, out var filteredTokensCount, out var minRelevancyCount, out var emptyCount))
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
                        foreach (var documentId in sortedIds[0].DocumentIds)
                        {
                            if (CommonIndex.TryGetPositionVector(documentId, out _, out var externalDocument))
                            {
                                const int metric = 1;
                                metricsCalculator.AppendReducedMetric(metric, searchVector, externalDocument);
                            }
                        }

                        break;
                    }
                default:
                    {
                        if (cancellationToken.IsCancellationRequested)
                            throw new OperationCanceledException(nameof(ReducedSearchGinArrayMergeFilter));

                        Process(sortedIds, searchVector, metricsCalculator,
                            filteredTokensCount, minRelevancyCount, emptyCount);

                        break;
                    }
            }
        }
        finally
        {
            TempStoragePool.InternalIdsWithTokenCollections.Return(sortedIds);
        }
    }

    private void Process(List<InternalDocumentIdsWithToken> sortedIds, TokenVector searchVector,
        IMetricsCalculator metricsCalculator, int filteredTokensCount,
        int minRelevancyCount, int emptyCount)
    {
        using DocumentIdsScoringIterator documentReducedScoreIterator = new(TempStoragePool,
            sortedIds, filteredTokensCount);

        using MetricsConsumer metricsConsumer = new(TempStoragePool,
            searchVector, metricsCalculator, CommonIndex, sortedIds, filteredTokensCount);

        documentReducedScoreIterator.IterateToObtainReducedMetric(metricsConsumer);
    }

    private readonly ref struct MetricsConsumer : DocumentIdsScoringIterator.IMetricsConsumer, IDisposable
    {
        private readonly TempStoragePool _tempStoragePool;
        private readonly TokenVector _searchVector;
        private readonly IMetricsCalculator _metricsCalculator;
        private readonly CommonIndex _commonIndex;
        private readonly List<DocumentIdsEnumerator> _list;

        public MetricsConsumer(TempStoragePool tempStoragePool, TokenVector searchVector,
            IMetricsCalculator metricsCalculator, CommonIndex commonIndex,
            List<InternalDocumentIdsWithToken> sortedIds, int filteredTokensCount)
        {
            _tempStoragePool = tempStoragePool;
            _searchVector = searchVector;
            _metricsCalculator = metricsCalculator;
            _commonIndex = commonIndex;

            _list = _tempStoragePool.InternalEnumeratorCollections.Get();

            for (var index = sortedIds.Count - 1; index >= filteredTokensCount; index--)
            {
                var docIdVector = sortedIds[index];
                _list.Add(docIdVector.DocumentIds.CreateDocumentListEnumerator());
            }

            for (var index = 0; index < _list.Count; index++)
            {
                CollectionsMarshal.AsSpan(_list)[index].MoveNext();
            }
        }

        public void Dispose()
        {
            _tempStoragePool.InternalEnumeratorCollections.Return(_list);
        }

        public void Accept(InternalDocumentId documentId, int score)
        {
            var counter = 1;

            for (var index = _list.Count - 1; index >= 0; index--)
            {
                ref var documentListEnumerator = ref CollectionsMarshal.AsSpan(_list)[index];

                if (documentListEnumerator.Current < documentId)
                {
                    if (documentListEnumerator.MoveNextBinarySearch(documentId))
                    {
                        if (documentListEnumerator.Current < documentId)
                        {
                            throw new InvalidOperationException();
                        }

                        if (documentListEnumerator.Current == documentId)
                        {
                            score++;

                            if (!documentListEnumerator.MoveNext())
                            {
                                _list.RemoveAt(index);
                            }
                        }
                    }
                    else
                    {
                        _list.RemoveAt(index);
                    }
                }
                else if (documentListEnumerator.Current == documentId)
                {
                    score++;

                    if (!documentListEnumerator.MoveNext())
                    {
                        _list.RemoveAt(index);
                    }
                }

                if (score <= counter)
                {
                    break;
                }

                counter++;
            }

            if (_commonIndex.TryGetPositionVector(documentId, out _, out var externalDocument))
            {
                _metricsCalculator.AppendReducedMetric(score, _searchVector, externalDocument);
            }
        }
    }
}

#endif
