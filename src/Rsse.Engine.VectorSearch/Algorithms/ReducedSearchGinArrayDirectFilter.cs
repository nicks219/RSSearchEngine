using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using RsseEngine.Contracts;
using RsseEngine.Dto;
using RsseEngine.Indexes;
using RsseEngine.Iterators;
using RsseEngine.Pools;
using RsseEngine.Processor;
using RsseEngine.SearchType;

namespace RsseEngine.Algorithms;

/// <summary>
/// Класс с алгоритмом подсчёта сокращенной метрики.
/// Метрика считается GIN индексе, применены дополнительные оптимизации.
/// </summary>
public readonly ref struct ReducedSearchGinArrayDirectFilter : IReducedSearchProcessor
{
    public required TempStoragePool TempStoragePool { private get; init; }

    /// <summary>
    /// Поддержка GIN-индекса.
    /// </summary>
    public required InvertedIndex InvertedIndex { private get; init; }

    public required GinRelevanceFilter RelevanceFilter { private get; init; }

    public required PositionSearchType PositionSearchType { private get; init; }

    /// <inheritdoc/>
    public void FindReduced(TokenVector searchVector, IMetricsCalculator metricsCalculator,
        CancellationToken cancellationToken)
    {
        var sortedIds = TempStoragePool.InternalDocumentIdListsWithTokenStorage.Get();

        try
        {
            if (!RelevanceFilter.FindFilteredDocumentsReducedMerge(InvertedIndex, searchVector,
                    sortedIds, out var filteredTokensCount, out var minRelevancyCount, out var emptyCount))
            {
                return;
            }

            if (cancellationToken.IsCancellationRequested)
                throw new OperationCanceledException(nameof(ReducedSearchGinArrayDirectFilter));

            switch (sortedIds.Count)
            {
                case 0:
                    {
                        break;
                    }
                case 1:
                    {
                        metricsCalculator.AppendReducedMetricsFromSingleIndex(searchVector,
                            InvertedIndex, sortedIds[0].DocumentIds);

                        break;
                    }
                default:
                    {
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

        using MetricsConsumer metricsConsumer = new(
            searchVector, metricsCalculator, InvertedIndex, sortedIds, filteredTokensCount,
            PositionSearchType, minRelevancyCount, emptyCount);

        documentReducedScoreIterator.Iterate(metricsConsumer);
    }

    private readonly ref struct MetricsConsumer : InternalDocumentReducedScoreIterator.IConsumer, IDisposable
    {
        private readonly TokenVector _searchVector;
        private readonly IMetricsCalculator _metricsCalculator;
        private readonly InvertedIndex _invertedIndex;

        private readonly List<InternalDocumentIdsWithToken> _sortedIds;
        private readonly int _filteredTokensCount;
        private readonly PositionSearchType _positionSearchType;
        private readonly int _minRelevancyCount;
        private readonly int _emptyCount;

        public MetricsConsumer(TokenVector searchVector,
            IMetricsCalculator metricsCalculator, InvertedIndex invertedIndex,
            List<InternalDocumentIdsWithToken> sortedIds, int filteredTokensCount,
            PositionSearchType positionSearchType, int minRelevancyCount, int emptyCount)
        {
            _searchVector = searchVector;
            _metricsCalculator = metricsCalculator;
            _invertedIndex = invertedIndex;

            _sortedIds = sortedIds;
            _filteredTokensCount = filteredTokensCount;
            _positionSearchType = positionSearchType;
            _minRelevancyCount = minRelevancyCount;
            _emptyCount = emptyCount;
        }

        public void Dispose()
        {
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public void Accept(InternalDocumentId documentId, int score)
        {
            if (!_invertedIndex.TryGetOffsetTokenVector(documentId, out var offsetTokenVector, out var externalDocument))
            {
                return;
            }

            var metric = score;

            switch (_positionSearchType)
            {
                case PositionSearchType.LinearScan:
                    {
                        var empty = _emptyCount;

                        for (var i = _filteredTokensCount; i < _sortedIds.Count; i++)
                        {
                            var token = _sortedIds[i].Token;

                            if (!offsetTokenVector.ContainsKeyLinearScan(token))
                            {
                                empty++;

                                if (empty > _searchVector.Count - _minRelevancyCount)
                                {
                                    return;
                                }
                            }
                            else
                            {
                                metric++;
                            }
                        }

                        _metricsCalculator.AppendReducedMetric(metric, _searchVector, externalDocument);

                        break;
                    }
                case PositionSearchType.BinarySearch:
                    {
                        var empty = _emptyCount;

                        for (var i = _filteredTokensCount; i < _sortedIds.Count; i++)
                        {
                            var token = _sortedIds[i].Token;

                            if (!offsetTokenVector.ContainsKeyBinarySearch(token))
                            {
                                empty++;

                                if (empty > _searchVector.Count - _minRelevancyCount)
                                {
                                    return;
                                }
                            }
                            else
                            {
                                metric++;
                            }
                        }

                        _metricsCalculator.AppendReducedMetric(metric, _searchVector, externalDocument);

                        break;
                    }
                default:
                    {
                        throw new NotSupportedException($"PositionSearchType {_positionSearchType} not supported.");
                    }
            }
        }
    }
}
