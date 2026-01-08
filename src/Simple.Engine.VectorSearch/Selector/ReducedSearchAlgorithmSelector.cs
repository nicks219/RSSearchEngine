using System;
using System.Threading;
using Rsse.Domain.Service.Configuration;
using SimpleEngine.Algorithms;
using SimpleEngine.Contracts;
using SimpleEngine.Dto.Common;
using SimpleEngine.Dto.Inverted;
using SimpleEngine.Indexes;
using SimpleEngine.Pools;
using SimpleEngine.Processor;
using SimpleEngine.SearchType;

namespace SimpleEngine.Selector;

/// <summary>
/// Компонент, предоставляющий доступ к различным алгоритмам reduced-поиска.
/// </summary>
[Obsolete("R&D only")]
public sealed class ReducedSearchAlgorithmSelector : ISearchAlgorithmSelector<ReducedSearchType, IReducedSearchProcessor>
{
    private readonly TempStoragePool _tempStoragePool;

    private readonly RelevanceFilter _relevanceFilter;

    private readonly DirectIndexLegacy _generalDirectIndexLegacyLegacy;

    private readonly CommonIndexes _partitions = new(IndexPoint.DictionaryStorageType.SortedArrayStorage);

    private readonly CommonIndexes _partitionsHs = new(IndexPoint.DictionaryStorageType.HashTableStorage);

    /// <summary>
    /// Компонент с reduced-алгоритмами.
    /// </summary>
    /// <param name="tempStoragePool">Пул коллекций.</param>
    /// <param name="generalDirectIndexLegacyLegacy">Общий индекс, используется в legacy-алгоритме.</param>
    /// <param name="relevancyThreshold">Порог релевантности</param>
    public ReducedSearchAlgorithmSelector(TempStoragePool tempStoragePool,
        DirectIndexLegacy generalDirectIndexLegacyLegacy, double relevancyThreshold)
    {
        // защита на случай изменения внешних проверок, до момента готовности алгоритмов
        EnvironmentReporter.ThrowIfProduction(nameof(ReducedSearchAlgorithmSelector));

        _tempStoragePool = tempStoragePool;
        _generalDirectIndexLegacyLegacy = generalDirectIndexLegacyLegacy;

        _relevanceFilter = new RelevanceFilter
        {
            Threshold = relevancyThreshold
        };
    }

    /// <inheritdoc/>
    public void Find(ReducedSearchType searchType, TokenVector searchVector,
        IMetricsCalculator metricsCalculator, CancellationToken cancellationToken)
    {
        // todo: именования состоят из набора параметров для алгоритмов ("как" а не "что") - можно добавить атрибуты
        // Offset в нейминге означает использование InvertedOffsetIndex, иначе InvertedIndex (UsesIndex)
        // Filter в нейминге означает использование GinRelevanceFilter (UsesFilter)
        // Linear - Binary - Hash в нейминге означают тип поиска в позициях токена (PositionSearchType)

        switch (searchType)
        {
            case ReducedSearchType.Legacy:
                {
                    RunLegacySearch(searchVector, metricsCalculator, cancellationToken);
                    break;
                }
            case ReducedSearchType.Direct:
                {
                    RunDirectSearch(searchVector, metricsCalculator, cancellationToken);
                    break;
                }
            case ReducedSearchType.MergeFilter:
                {
                    RunMergeFilterSearch(searchVector, metricsCalculator, cancellationToken);
                    break;
                }
            case ReducedSearchType.DirectFilterLinear:
                {
                    RunDirectFilterLinearSearch(searchVector, metricsCalculator, cancellationToken);
                    break;
                }
            case ReducedSearchType.DirectFilterBinary:
                {
                    RunDirectFilterBinarySearch(searchVector, metricsCalculator, cancellationToken);
                    break;
                }
            case ReducedSearchType.DirectFilterHash:
                {
                    RunDirectFilterHashSearch(searchVector, metricsCalculator, cancellationToken);
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
        _partitions.AddOrUpdateVector(documentId, tokenVector);
        _partitionsHs.AddOrUpdateVector(documentId, tokenVector);
    }

    /// <inheritdoc/>
    public void UpdateVector(DocumentId documentId, TokenVector tokenVector)
    {
        _partitions.AddOrUpdateVector(documentId, tokenVector);
        _partitionsHs.AddOrUpdateVector(documentId, tokenVector);
    }

    /// <inheritdoc/>
    public void RemoveVector(DocumentId documentId, TokenVector tokenVector)
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

    private void RunLegacySearch(TokenVector searchVector, IMetricsCalculator metricsCalculator,
        CancellationToken cancellationToken)
    {
        var reducedSearchLegacy = new ReducedSearchLegacy
        {
            GeneralDirectIndexLegacy = _generalDirectIndexLegacyLegacy
        };

        reducedSearchLegacy.FindReduced(searchVector, metricsCalculator, cancellationToken);
    }

    private void RunDirectSearch(TokenVector searchVector, IMetricsCalculator metricsCalculator,
        CancellationToken cancellationToken)
    {
        foreach (var invertedIndex in _partitions.Indices)
        {
            var reducedSearchGinArrayDirect = new ReducedSearchGinArrayDirect
            {
                TempStoragePool = _tempStoragePool,
                CommonIndex = invertedIndex,
            };

            reducedSearchGinArrayDirect.FindReduced(searchVector, metricsCalculator, cancellationToken);
        }
    }

    private void RunMergeFilterSearch(TokenVector searchVector, IMetricsCalculator metricsCalculator,
        CancellationToken cancellationToken)
    {
        foreach (var invertedIndex in _partitions.Indices)
        {
            var reducedSearchGinArrayMergeFilter = new ReducedSearchGinArrayMergeFilter
            {
                TempStoragePool = _tempStoragePool,
                CommonIndex = invertedIndex,
                RelevanceFilter = _relevanceFilter
            };

            reducedSearchGinArrayMergeFilter.FindReduced(searchVector, metricsCalculator, cancellationToken);
        }
    }

    private void RunDirectFilterLinearSearch(TokenVector searchVector, IMetricsCalculator metricsCalculator,
        CancellationToken cancellationToken)
    {
        foreach (var invertedIndex in _partitions.Indices)
        {
            var reducedSearchGinArrayDirectFilterLs = new ReducedSearchGinArrayDirectFilter
            {
                TempStoragePool = _tempStoragePool,
                CommonIndex = invertedIndex,
                RelevanceFilter = _relevanceFilter,
                PositionSearchType = PositionSearchType.LinearScan
            };

            reducedSearchGinArrayDirectFilterLs.FindReduced(searchVector, metricsCalculator, cancellationToken);
        }
    }

    private void RunDirectFilterBinarySearch(TokenVector searchVector, IMetricsCalculator metricsCalculator,
        CancellationToken cancellationToken)
    {
        foreach (var invertedIndex in _partitions.Indices)
        {
            var reducedSearchGinArrayDirectFilterBs = new ReducedSearchGinArrayDirectFilter
            {
                TempStoragePool = _tempStoragePool,
                CommonIndex = invertedIndex,
                RelevanceFilter = _relevanceFilter,
                PositionSearchType = PositionSearchType.BinarySearch
            };

            reducedSearchGinArrayDirectFilterBs.FindReduced(searchVector, metricsCalculator, cancellationToken);
        }
    }

    private void RunDirectFilterHashSearch(TokenVector searchVector, IMetricsCalculator metricsCalculator,
        CancellationToken cancellationToken)
    {
        foreach (var invertedIndexHs in _partitionsHs.Indices)
        {
            var reducedSearchGinArrayDirectFilterHs = new ReducedSearchGinArrayDirectFilter
            {
                TempStoragePool = _tempStoragePool,
                CommonIndex = invertedIndexHs,
                RelevanceFilter = _relevanceFilter,
                PositionSearchType = PositionSearchType.LinearScan
            };

            reducedSearchGinArrayDirectFilterHs.FindReduced(searchVector, metricsCalculator, cancellationToken);
        }
    }
}
