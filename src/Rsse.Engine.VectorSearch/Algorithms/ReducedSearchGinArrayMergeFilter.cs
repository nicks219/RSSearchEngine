using System;
using System.Collections.Generic;
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
/// Класс с алгоритмом подсчёта сокращенной метрики.
/// Метрика считается GIN индексе, применены дополнительные оптимизации.
/// </summary>
public sealed class ReducedSearchGinArrayMergeFilter : IReducedSearchProcessor
{
    public required TempStoragePool TempStoragePool { private get; init; }

    /// <summary>
    /// Поддержка GIN-индекса.
    /// </summary>
    public required ArrayDirectOffsetIndex GinReduced { private get; init; }

    public required GinRelevanceFilter RelevanceFilter { private get; init; }

    /// <inheritdoc/>
    public void FindReduced(TokenVector searchVector, IMetricsCalculator metricsCalculator,
        CancellationToken cancellationToken)
    {
        var sortedIds = TempStoragePool.InternalDocumentIdListsWithTokenStorage.Get();

        try
        {
            if (!RelevanceFilter.FindFilteredDocumentsReducedMerge(GinReduced, searchVector,
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
                            if (GinReduced.TryGetOffsetTokenVector(documentId, out _, out var externalDocument))
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
            TempStoragePool.InternalDocumentIdListsWithTokenStorage.Return(sortedIds);
        }
    }

    private void Process(List<InternalDocumentIdsWithToken> sortedIds, TokenVector searchVector,
        IMetricsCalculator metricsCalculator, int filteredTokensCount,
        int minRelevancyCount, int emptyCount)
    {
        using InternalDocumentReducedScoreIterator documentReducedScoreIterator = new(TempStoragePool,
            sortedIds, filteredTokensCount);

        using MetricsConsumer metricsConsumer = new(TempStoragePool,
            searchVector, metricsCalculator, GinReduced, sortedIds, filteredTokensCount);

        documentReducedScoreIterator.Iterate(metricsConsumer);
    }

    private readonly ref struct MetricsConsumer : InternalDocumentReducedScoreIterator.IConsumer, IDisposable
    {
        private readonly TempStoragePool _tempStoragePool;
        private readonly TokenVector _searchVector;
        private readonly IMetricsCalculator _metricsCalculator;
        private readonly ArrayDirectOffsetIndex _ginReduced;
        private readonly List<InternalDocumentListEnumerator> _list;

        public MetricsConsumer(TempStoragePool tempStoragePool, TokenVector searchVector,
            IMetricsCalculator metricsCalculator, ArrayDirectOffsetIndex ginReduced,
            List<InternalDocumentIdsWithToken> sortedIds, int filteredTokensCount)
        {
            _tempStoragePool = tempStoragePool;
            _searchVector = searchVector;
            _metricsCalculator = metricsCalculator;
            _ginReduced = ginReduced;

            _list = _tempStoragePool.ListInternalEnumeratorListsStorage.Get();

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
            _tempStoragePool.ListInternalEnumeratorListsStorage.Return(_list);
        }

        public void Accept(InternalDocumentId documentId, int score)
        {
            var counter = 1;

            for (var index = _list.Count - 1; index >= 0; index--)
            {
                ref var documentListEnumerator = ref CollectionsMarshal.AsSpan(_list)[index];

                if (documentListEnumerator.Current.Value < documentId.Value)
                {
                    if (documentListEnumerator.MoveNextBinarySearch(documentId))
                    {
                        if (documentListEnumerator.Current.Value < documentId.Value)
                        {
                            throw new InvalidOperationException();
                        }

                        if (documentListEnumerator.Current.Value == documentId.Value)
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
                else if (documentListEnumerator.Current.Value == documentId.Value)
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

            if (_ginReduced.TryGetOffsetTokenVector(documentId, out _, out var externalDocument))
            {
                _metricsCalculator.AppendReducedMetric(score, _searchVector, externalDocument);
            }
        }
    }
}
