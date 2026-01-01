using System;
using System.Threading;
using Rsse.Domain.Service.Configuration;
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
/// Компонент, предоставляющий доступ к различным алгоритмам extended-поиска.
/// </summary>
public sealed class ExtendedSearchAlgorithmSelector
    : ISearchAlgorithmSelector<ExtendedSearchType, IExtendedSearchProcessor>
{
    private readonly TempStoragePool _tempStoragePool;

    private readonly GinRelevanceFilter _relevanceFilter;

    private readonly DirectIndex _generalDirectIndex;

    private readonly InvertedOffsetIndexPartitions _offsetPartitions = new();

    private readonly InvertedIndexPartitions _partitions = new(DocumentDataPoint.DocumentDataPointSearchType.BinaryTree);

    private readonly InvertedIndexPartitions _partitionsHs = new(DocumentDataPoint.DocumentDataPointSearchType.HashMap);

    /// <summary>
    /// Компонент с extended-алгоритмами.
    /// </summary>
    /// <param name="tempStoragePool">Пул коллекций.</param>
    /// <param name="generalDirectIndex">Общий индекс.</param>
    /// <param name="relevancyThreshold">Порог релевантности.</param>
    public ExtendedSearchAlgorithmSelector(TempStoragePool tempStoragePool,
        DirectIndex generalDirectIndex, double relevancyThreshold)
    {
        // защита на случай изменения внешних проверок, до момента готовности алгоритмов
        EnvironmentReporter.ThrowIfProduction(nameof(ExtendedSearchAlgorithmSelector));

        _tempStoragePool = tempStoragePool;
        _generalDirectIndex = generalDirectIndex;

        _relevanceFilter = new GinRelevanceFilter
        {
            Threshold = relevancyThreshold
        };
    }

    /// <inheritdoc/>
    public void Find(ExtendedSearchType searchType, TokenVector searchVector,
        IMetricsCalculator metricsCalculator, CancellationToken cancellationToken)
    {
        switch (searchType)
        {
            case ExtendedSearchType.Legacy:
                {
                    FindExtendedLegacy(searchVector, metricsCalculator, cancellationToken);
                    break;
                }
            case ExtendedSearchType.GinOffset:
                {
                    FindExtendedGinOffset(searchVector, metricsCalculator, cancellationToken);
                    break;
                }
            case ExtendedSearchType.GinOffsetFilter:
                {
                    FindExtendedGinOffsetFilter(searchVector, metricsCalculator, cancellationToken);
                    break;
                }
            case ExtendedSearchType.GinArrayDirectLs:
                {
                    FindExtendedGinArrayDirectLs(searchVector, metricsCalculator, cancellationToken);
                    break;
                }
            case ExtendedSearchType.GinArrayDirectFilterLs:
                {
                    FindExtendedGinArrayDirectFilterLs(searchVector, metricsCalculator, cancellationToken);
                    break;
                }
            case ExtendedSearchType.GinArrayDirectBs:
                {
                    FindExtendedGinArrayDirectBs(searchVector, metricsCalculator, cancellationToken);
                    break;
                }
            case ExtendedSearchType.GinArrayDirectFilterBs:
                {
                    FindExtendedGinArrayDirectFilterBs(searchVector, metricsCalculator, cancellationToken);
                    break;
                }
            case ExtendedSearchType.GinArrayDirectHs:
                {
                    FindExtendedGinArrayDirectHs(searchVector, metricsCalculator, cancellationToken);
                    break;
                }
            case ExtendedSearchType.GinArrayDirectFilterHs:
                {
                    FindExtendedGinArrayDirectFilterHs(searchVector, metricsCalculator, cancellationToken);
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
        _offsetPartitions.AddOrUpdateVector(documentId, tokenVector);
        _partitions.AddOrUpdateVector(documentId, tokenVector);
        _partitionsHs.AddOrUpdateVector(documentId, tokenVector);
    }

    /// <inheritdoc/>
    public void UpdateVector(DocumentId documentId, TokenVector tokenVector)
    {
        _offsetPartitions.AddOrUpdateVector(documentId, tokenVector);
        _partitions.AddOrUpdateVector(documentId, tokenVector);
        _partitionsHs.AddOrUpdateVector(documentId, tokenVector);
    }

    /// <inheritdoc/>
    public void RemoveVector(DocumentId documentId, TokenVector tokenVector)
    {
        _offsetPartitions.RemoveVector(documentId);
        _partitions.RemoveVector(documentId);
        _partitionsHs.RemoveVector(documentId);
    }

    /// <inheritdoc/>
    public void Clear()
    {
        _offsetPartitions.Clear();
        _partitions.Clear();
        _partitionsHs.Clear();
    }

    private void FindExtendedLegacy(TokenVector searchVector, IMetricsCalculator metricsCalculator,
        CancellationToken cancellationToken)
    {
        var extendedSearchLegacy = new ExtendedSearchLegacy
        {
            GeneralDirectIndex = _generalDirectIndex
        };

        extendedSearchLegacy.FindExtended(searchVector, metricsCalculator, cancellationToken);
    }

    private void FindExtendedGinOffset(TokenVector searchVector, IMetricsCalculator metricsCalculator,
        CancellationToken cancellationToken)
    {
        foreach (var invertedOffsetIndex in _offsetPartitions.Indices)
        {
            var extendedSearchGinOffset = new ExtendedSearchGinOffset
            {
                TempStoragePool = _tempStoragePool,
                GinExtended = invertedOffsetIndex
            };

            extendedSearchGinOffset.FindExtended(searchVector, metricsCalculator, cancellationToken);
        }
    }

    private void FindExtendedGinOffsetFilter(TokenVector searchVector, IMetricsCalculator metricsCalculator,
        CancellationToken cancellationToken)
    {
        foreach (var invertedOffsetIndex in _offsetPartitions.Indices)
        {
            var extendedSearchGinOffsetFilter = new ExtendedSearchGinOffsetFilter
            {
                TempStoragePool = _tempStoragePool,
                GinExtended = invertedOffsetIndex,
                RelevanceFilter = _relevanceFilter
            };

            extendedSearchGinOffsetFilter.FindExtended(searchVector, metricsCalculator, cancellationToken);
        }
    }

    private void FindExtendedGinArrayDirectLs(TokenVector searchVector, IMetricsCalculator metricsCalculator,
        CancellationToken cancellationToken)
    {
        foreach (var invertedIndex in _partitions.Indices)
        {
            var extendedSearchGinArrayDirectLs = new ExtendedSearchGinArrayDirect
            {
                TempStoragePool = _tempStoragePool,
                InvertedIndex = invertedIndex,
                PositionSearchType = PositionSearchType.LinearScan
            };

            extendedSearchGinArrayDirectLs.FindExtended(searchVector, metricsCalculator, cancellationToken);
        }
    }

    private void FindExtendedGinArrayDirectFilterLs(TokenVector searchVector, IMetricsCalculator metricsCalculator,
        CancellationToken cancellationToken)
    {
        foreach (var invertedIndex in _partitions.Indices)
        {
            var extendedSearchGinArrayDirectFilterLs = new ExtendedSearchGinArrayDirectFilter
            {
                TempStoragePool = _tempStoragePool,
                InvertedIndex = invertedIndex,
                RelevanceFilter = _relevanceFilter,
                PositionSearchType = PositionSearchType.LinearScan
            };

            extendedSearchGinArrayDirectFilterLs.FindExtended(searchVector, metricsCalculator, cancellationToken);
        }
    }

    private void FindExtendedGinArrayDirectBs(TokenVector searchVector, IMetricsCalculator metricsCalculator,
        CancellationToken cancellationToken)
    {
        foreach (var invertedIndex in _partitions.Indices)
        {
            var extendedSearchGinArrayDirectBs = new ExtendedSearchGinArrayDirect
            {
                TempStoragePool = _tempStoragePool,
                InvertedIndex = invertedIndex,
                PositionSearchType = PositionSearchType.BinarySearch
            };

            extendedSearchGinArrayDirectBs.FindExtended(searchVector, metricsCalculator, cancellationToken);
        }
    }

    private void FindExtendedGinArrayDirectFilterBs(TokenVector searchVector, IMetricsCalculator metricsCalculator,
        CancellationToken cancellationToken)
    {
        foreach (var invertedIndex in _partitions.Indices)
        {
            var extendedSearchGinArrayDirectFilterBs = new ExtendedSearchGinArrayDirectFilter
            {
                TempStoragePool = _tempStoragePool,
                InvertedIndex = invertedIndex,
                RelevanceFilter = _relevanceFilter,
                PositionSearchType = PositionSearchType.BinarySearch
            };

            extendedSearchGinArrayDirectFilterBs.FindExtended(searchVector, metricsCalculator, cancellationToken);
        }
    }

    private void FindExtendedGinArrayDirectHs(TokenVector searchVector, IMetricsCalculator metricsCalculator,
        CancellationToken cancellationToken)
    {
        foreach (var invertedIndexHs in _partitionsHs.Indices)
        {
            var extendedSearchGinArrayDirectHs = new ExtendedSearchGinArrayDirect
            {
                TempStoragePool = _tempStoragePool,
                InvertedIndex = invertedIndexHs,
                PositionSearchType = PositionSearchType.LinearScan
            };

            extendedSearchGinArrayDirectHs.FindExtended(searchVector, metricsCalculator, cancellationToken);
        }
    }

    private void FindExtendedGinArrayDirectFilterHs(TokenVector searchVector, IMetricsCalculator metricsCalculator,
        CancellationToken cancellationToken)
    {
        foreach (var invertedIndexHs in _partitionsHs.Indices)
        {
            var extendedSearchGinArrayDirectFilterHs = new ExtendedSearchGinArrayDirectFilter
            {
                TempStoragePool = _tempStoragePool,
                InvertedIndex = invertedIndexHs,
                RelevanceFilter = _relevanceFilter,
                PositionSearchType = PositionSearchType.LinearScan
            };

            extendedSearchGinArrayDirectFilterHs.FindExtended(searchVector, metricsCalculator, cancellationToken);
        }
    }
}
