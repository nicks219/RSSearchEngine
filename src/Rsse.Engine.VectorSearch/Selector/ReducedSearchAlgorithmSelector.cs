using System;
using System.Threading;
using RsseEngine.Algorithms;
using RsseEngine.Contracts;
using RsseEngine.Dto;
using RsseEngine.Dto.Offsets;
using RsseEngine.Indexes;
using RsseEngine.Pools;
using RsseEngine.Processor;
using RsseEngine.SearchType;

namespace RsseEngine.Selector;

/// <summary>
/// Компонент, предоставляющий доступ к различным алгоритмам reduced-поиска.
/// </summary>
public sealed class ReducedSearchAlgorithmSelector
    : ISearchAlgorithmSelector<ReducedSearchType, IReducedSearchProcessor>
{
    private readonly SearchIndexType _searchIndexType;

    private readonly TempStoragePool _tempStoragePool;

    private readonly GinRelevanceFilter _relevanceFilter;

    private readonly DirectIndex _generalDirectIndex;

    private readonly InvertedIndexPartitions _partitions = new(DocumentDataPoint.DocumentDataPointSearchType.BinaryTree);

    private readonly InvertedIndexPartitions _partitionsHs = new(DocumentDataPoint.DocumentDataPointSearchType.HashMap);

    /// <summary>
    /// Компонент с reduced-алгоритмами.
    /// </summary>
    /// <param name="searchIndexType">Тип поискового индекса.</param>
    /// <param name="tempStoragePool">Пул коллекций.</param>
    /// <param name="generalDirectIndex">Общий индекс.</param>
    /// <param name="relevancyThreshold">Порог релевантности</param>
    public ReducedSearchAlgorithmSelector(SearchIndexType searchIndexType, TempStoragePool tempStoragePool,
        DirectIndex generalDirectIndex, double relevancyThreshold)
    {
        _searchIndexType = searchIndexType;
        _tempStoragePool = tempStoragePool;
        _generalDirectIndex = generalDirectIndex;

        _relevanceFilter = new GinRelevanceFilter
        {
            Threshold = relevancyThreshold
        };
    }

    /// <inheritdoc/>
    public void Find(ReducedSearchType searchType, TokenVector searchVector,
        IMetricsCalculator metricsCalculator, CancellationToken cancellationToken)
    {
        switch (searchType)
        {
            case ReducedSearchType.Legacy:
                {
                    FindReducedLegacy(searchVector, metricsCalculator, cancellationToken);
                    break;
                }
            case ReducedSearchType.GinArrayDirect:
                {
                    FindReducedGinArrayDirect(searchVector, metricsCalculator, cancellationToken);
                    break;
                }
            case ReducedSearchType.GinArrayMergeFilter:
                {
                    FindReducedGinArrayMergeFilter(searchVector, metricsCalculator, cancellationToken);
                    break;
                }
            case ReducedSearchType.GinArrayDirectFilterLs:
                {
                    FindReducedGinArrayDirectFilterLs(searchVector, metricsCalculator, cancellationToken);
                    break;
                }
            case ReducedSearchType.GinArrayDirectFilterBs:
                {
                    FindReducedGinArrayDirectFilterBs(searchVector, metricsCalculator, cancellationToken);
                    break;
                }
            case ReducedSearchType.GinArrayDirectFilterHs:
                {
                    FindReducedGinArrayDirectFilterHs(searchVector, metricsCalculator, cancellationToken);
                    break;
                }
            default:
                {
                    throw new ArgumentOutOfRangeException(nameof(searchType), searchType, "unknown search type");
                }
        }
    }

    /// <inheritdoc/>
    public void AddVector(DocumentId documentId, TokenVector tokenVector)
    {
        if ((_searchIndexType & SearchIndexType.InvertedIndexReduced) != 0)
        {
            _partitions.AddOrUpdateVector(documentId, tokenVector);
        }

        if ((_searchIndexType & SearchIndexType.InvertedIndexHsReduced) != 0)
        {
            _partitionsHs.AddOrUpdateVector(documentId, tokenVector);
        }
    }

    /// <inheritdoc/>
    public void UpdateVector(DocumentId documentId, TokenVector tokenVector)
    {
        if ((_searchIndexType & SearchIndexType.InvertedIndexReduced) != 0)
        {
            _partitions.AddOrUpdateVector(documentId, tokenVector);
        }

        if ((_searchIndexType & SearchIndexType.InvertedIndexHsReduced) != 0)
        {
            _partitionsHs.AddOrUpdateVector(documentId, tokenVector);
        }
    }

    /// <inheritdoc/>
    public void RemoveVector(DocumentId documentId)
    {
        _partitions.RemoveVector(documentId);
        _partitionsHs.RemoveVector(documentId);
    }

    /// <inheritdoc/>
    public void Clear()
    {
        _partitions.Clear();
        _partitionsHs.Clear();
    }

    private void FindReducedLegacy(TokenVector searchVector, IMetricsCalculator metricsCalculator,
        CancellationToken cancellationToken)
    {
        if ((_searchIndexType & SearchIndexType.Direct) == 0)
        {
            return;
        }

        var reducedSearchLegacy = new ReducedSearchLegacy
        {
            GeneralDirectIndex = _generalDirectIndex
        };

        reducedSearchLegacy.FindReduced(searchVector, metricsCalculator, cancellationToken);
    }

    private void FindReducedGinArrayDirect(TokenVector searchVector, IMetricsCalculator metricsCalculator,
        CancellationToken cancellationToken)
    {
        if ((_searchIndexType & SearchIndexType.InvertedIndexReduced) == 0)
        {
            return;
        }

        foreach (var invertedIndex in _partitions.Indices)
        {
            var reducedSearchGinArrayDirect = new ReducedSearchGinArrayDirect
            {
                TempStoragePool = _tempStoragePool,
                InvertedIndex = invertedIndex,
            };

            reducedSearchGinArrayDirect.FindReduced(searchVector, metricsCalculator, cancellationToken);
        }
    }

    private void FindReducedGinArrayMergeFilter(TokenVector searchVector, IMetricsCalculator metricsCalculator,
        CancellationToken cancellationToken)
    {
        if ((_searchIndexType & SearchIndexType.InvertedIndexReduced) == 0)
        {
            return;
        }

        foreach (var invertedIndex in _partitions.Indices)
        {
            var reducedSearchGinArrayMergeFilter = new ReducedSearchGinArrayMergeFilter
            {
                TempStoragePool = _tempStoragePool,
                InvertedIndex = invertedIndex,
                RelevanceFilter = _relevanceFilter
            };

            reducedSearchGinArrayMergeFilter.FindReduced(searchVector, metricsCalculator, cancellationToken);
        }
    }

    private void FindReducedGinArrayDirectFilterLs(TokenVector searchVector, IMetricsCalculator metricsCalculator,
        CancellationToken cancellationToken)
    {
        if ((_searchIndexType & SearchIndexType.InvertedIndexReduced) == 0)
        {
            return;
        }

        foreach (var invertedIndex in _partitions.Indices)
        {
            var reducedSearchGinArrayDirectFilterLs = new ReducedSearchGinArrayDirectFilter
            {
                TempStoragePool = _tempStoragePool,
                InvertedIndex = invertedIndex,
                RelevanceFilter = _relevanceFilter,
                PositionSearchType = PositionSearchType.LinearScan
            };

            reducedSearchGinArrayDirectFilterLs.FindReduced(searchVector, metricsCalculator, cancellationToken);
        }
    }

    private void FindReducedGinArrayDirectFilterBs(TokenVector searchVector, IMetricsCalculator metricsCalculator,
        CancellationToken cancellationToken)
    {
        if ((_searchIndexType & SearchIndexType.InvertedIndexReduced) == 0)
        {
            return;
        }

        foreach (var invertedIndex in _partitions.Indices)
        {
            var reducedSearchGinArrayDirectFilterBs = new ReducedSearchGinArrayDirectFilter
            {
                TempStoragePool = _tempStoragePool,
                InvertedIndex = invertedIndex,
                RelevanceFilter = _relevanceFilter,
                PositionSearchType = PositionSearchType.BinarySearch
            };

            reducedSearchGinArrayDirectFilterBs.FindReduced(searchVector, metricsCalculator, cancellationToken);
        }
    }

    private void FindReducedGinArrayDirectFilterHs(TokenVector searchVector, IMetricsCalculator metricsCalculator,
        CancellationToken cancellationToken)
    {
        if ((_searchIndexType & SearchIndexType.InvertedIndexHsReduced) == 0)
        {
            return;
        }

        foreach (var invertedIndexHs in _partitionsHs.Indices)
        {
            var reducedSearchGinArrayDirectFilterHs = new ReducedSearchGinArrayDirectFilter
            {
                TempStoragePool = _tempStoragePool,
                InvertedIndex = invertedIndexHs,
                RelevanceFilter = _relevanceFilter,
                PositionSearchType = PositionSearchType.LinearScan
            };

            reducedSearchGinArrayDirectFilterHs.FindReduced(searchVector, metricsCalculator, cancellationToken);
        }
    }
}
