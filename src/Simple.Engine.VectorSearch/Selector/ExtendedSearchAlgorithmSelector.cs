using System;
using System.Threading;
using Rsse.Domain.Service.Configuration;
using SimpleEngine.Algorithms;
using SimpleEngine.Algorithms.Legacy;
using SimpleEngine.Contracts;
using SimpleEngine.Dto.Common;
using SimpleEngine.Dto.Inverted;
using SimpleEngine.Indexes;
using SimpleEngine.Pools;
using SimpleEngine.SearchType;

namespace SimpleEngine.Selector;

/// <summary>
/// Компонент, предоставляющий доступ к различным алгоритмам extended-поиска.
/// </summary>
[Obsolete("R&D only")]
public sealed class ExtendedSearchAlgorithmSelector
    : ISearchAlgorithmSelector<ExtendedSearchType, IExtendedSearchProcessor>
{
    private readonly TempStoragePool _tempStoragePool;

    // private readonly RelevanceFilter _relevanceFilter;

    private readonly GeneralDirectIndexLegacy _generalDirectIndexLegacy;

    // private readonly InvertedOffsetIndexes _offsetPartitions = new();

    private readonly CommonIndices _partitions = new(IndexPoint.DictionaryStorageType.SortedArrayStorage);

    private readonly CommonIndices _partitionsHs = new(IndexPoint.DictionaryStorageType.HashTableStorage);

    private readonly InvertedIndexLegacy _invertedIndexLegacy = new();

    /// <summary>
    /// Компонент с extended-алгоритмами.
    /// </summary>
    /// <param name="tempStoragePool">Пул коллекций.</param>
    /// <param name="generalDirectIndexLegacy">Общий индекс, используется в legacy-алгоритме.</param>
    /// <param name="relevancyThreshold">Порог релевантности.</param>
    public ExtendedSearchAlgorithmSelector(TempStoragePool tempStoragePool,
        GeneralDirectIndexLegacy generalDirectIndexLegacy, double relevancyThreshold)
    {
        // защита на случай изменения внешних проверок, до момента готовности алгоритмов
        EnvironmentReporter.ThrowIfProduction(nameof(ExtendedSearchAlgorithmSelector));

        _tempStoragePool = tempStoragePool;
        _generalDirectIndexLegacy = generalDirectIndexLegacy;

        /*_relevanceFilter = new RelevanceFilter
        {
            Threshold = relevancyThreshold
        };*/
    }

    /// <inheritdoc/>
    public void Find(ExtendedSearchType searchType, TokenVector searchVector,
        IMetricsCalculator metricsCalculator, CancellationToken cancellationToken)
    {
        // todo: именования состоят из набора параметров для алгоритмов ("как" а не "что") - можно добавить атрибуты
        // Offset в нейминге означает использование InvertedOffsetIndex, иначе InvertedIndex (UsesIndex)
        // Filter в нейминге означает использование GinRelevanceFilter (UsesFilter)
        // Linear - Binary - Hash в нейминге означают тип поиска в позициях токена (PositionSearchType)

        switch (searchType)
        {
            case ExtendedSearchType.Legacy:
                {
                    RunLegacySearch(searchVector, metricsCalculator, cancellationToken);
                    break;
                }
            case ExtendedSearchType.SimpleLegacy:
            {
                RunSimpleLegacySearch(searchVector, metricsCalculator, cancellationToken);
                break;
            }
            /*case ExtendedSearchType.Offset:
                {
                    RunOffsetSearch(searchVector, metricsCalculator, cancellationToken);
                    break;
                }
            case ExtendedSearchType.OffsetFilter:
                {
                    RunOffsetFilterSearch(searchVector, metricsCalculator, cancellationToken);
                    break;
                }*/
            case ExtendedSearchType.DirectLinear:
                {
                    RunDirectLinearSearch(searchVector, metricsCalculator, cancellationToken);
                    break;
                }
            /*case ExtendedSearchType.DirectFilterLinear:
                {
                    RunDirectFilterLinearSearch(searchVector, metricsCalculator, cancellationToken);
                    break;
                }*/
            case ExtendedSearchType.DirectBinary:
                {
                    RunDirectBinarySearch(searchVector, metricsCalculator, cancellationToken);
                    break;
                }
            /*case ExtendedSearchType.DirectFilterBinary:
                {
                    RunDirectFilterBinarySearch(searchVector, metricsCalculator, cancellationToken);
                    break;
                }*/
            case ExtendedSearchType.DirectHash:
                {
                    RunDirectHashSearch(searchVector, metricsCalculator, cancellationToken);
                    break;
                }
            /*case ExtendedSearchType.DirectFilterHash:
                {
                    RunDirectFilterHashSearch(searchVector, metricsCalculator, cancellationToken);
                    break;
                }*/
            default:
                {
                    throw new ArgumentOutOfRangeException(nameof(searchType), searchType, "unknown search type");
                }
        }
    }

    /// <inheritdoc/>
    public void AddVector(DocumentId documentId, TokenVector tokenVector)
    {
        //_offsetPartitions.AddOrUpdateVector(documentId, tokenVector);
        _partitions.AddOrUpdateVector(documentId, tokenVector);
        _partitionsHs.AddOrUpdateVector(documentId, tokenVector);

        _invertedIndexLegacy.TryAdd(documentId, tokenVector);
    }

    /// <inheritdoc/>
    public void UpdateVector(DocumentId documentId, TokenVector tokenVector)
    {
        //_offsetPartitions.AddOrUpdateVector(documentId, tokenVector);
        _partitions.AddOrUpdateVector(documentId, tokenVector);
        _partitionsHs.AddOrUpdateVector(documentId, tokenVector);

        var oldTokenLine = _generalDirectIndexLegacy[documentId];
        _invertedIndexLegacy.TryUpdate(documentId, tokenVector, oldTokenLine.Extended);
    }

    /// <inheritdoc/>
    public void RemoveVector(DocumentId documentId, TokenVector tokenVector)
    {
        //_offsetPartitions.RemoveVector(documentId);
        _partitions.RemoveVector(documentId);
        _partitionsHs.RemoveVector(documentId);

        _invertedIndexLegacy.TryRemoveDocumentId(documentId, tokenVector);
    }

    /// <inheritdoc/>
    public void Clear()
    {
        //_offsetPartitions.Clear();
        _partitions.Clear();
        _partitionsHs.Clear();

        _invertedIndexLegacy.Clear();
    }

    private void RunLegacySearch(TokenVector searchVector, IMetricsCalculator metricsCalculator,
        CancellationToken cancellationToken)
    {
        var extendedSearchLegacy = new ExtendedSearchLegacy
        {
            GeneralDirectIndexLegacy = _generalDirectIndexLegacy
        };

        extendedSearchLegacy.FindExtended(searchVector, metricsCalculator, cancellationToken);
    }

    private void RunSimpleLegacySearch(TokenVector searchVector, IMetricsCalculator metricsCalculator,
        CancellationToken cancellationToken)
    {
        var extendedSearchLegacy = new ExtendedSearchSimple
        {
            GeneralDirectIndexLegacy = _generalDirectIndexLegacy,
            InvertedIndexLegacy = _invertedIndexLegacy,
            TempStoragePool = _tempStoragePool
        };

        extendedSearchLegacy.FindExtended(searchVector, metricsCalculator, cancellationToken);
    }

    /*private void RunOffsetSearch(TokenVector searchVector, IMetricsCalculator metricsCalculator,
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

    private void RunOffsetFilterSearch(TokenVector searchVector, IMetricsCalculator metricsCalculator,
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
    }*/

    private void RunDirectLinearSearch(TokenVector searchVector, IMetricsCalculator metricsCalculator,
        CancellationToken cancellationToken)
    {
        foreach (var invertedIndex in _partitions.Indices)
        {
            var extendedSearchGinArrayDirectLs = new ExtendedSearchGinArrayDirect
            {
                TempStoragePool = _tempStoragePool,
                CommonIndex = invertedIndex,
                PositionSearchType = PositionSearchType.LinearScan
            };

            extendedSearchGinArrayDirectLs.FindExtended(searchVector, metricsCalculator, cancellationToken);
        }
    }

    /*private void RunDirectFilterLinearSearch(TokenVector searchVector, IMetricsCalculator metricsCalculator,
        CancellationToken cancellationToken)
    {
        foreach (var invertedIndex in _partitions.Indices)
        {
            var extendedSearchGinArrayDirectFilterLs = new ExtendedSearchGinArrayDirectFilter
            {
                TempStoragePool = _tempStoragePool,
                CommonIndex = invertedIndex,
                RelevanceFilter = _relevanceFilter,
                PositionSearchType = PositionSearchType.LinearScan
            };

            extendedSearchGinArrayDirectFilterLs.FindExtended(searchVector, metricsCalculator, cancellationToken);
        }
    }*/

    private void RunDirectBinarySearch(TokenVector searchVector, IMetricsCalculator metricsCalculator,
        CancellationToken cancellationToken)
    {
        foreach (var invertedIndex in _partitions.Indices)
        {
            var extendedSearchGinArrayDirectBs = new ExtendedSearchGinArrayDirect
            {
                TempStoragePool = _tempStoragePool,
                CommonIndex = invertedIndex,
                PositionSearchType = PositionSearchType.BinarySearch
            };

            extendedSearchGinArrayDirectBs.FindExtended(searchVector, metricsCalculator, cancellationToken);
        }
    }

    /*private void RunDirectFilterBinarySearch(TokenVector searchVector, IMetricsCalculator metricsCalculator,
        CancellationToken cancellationToken)
    {
        foreach (var invertedIndex in _partitions.Indices)
        {
            var extendedSearchGinArrayDirectFilterBs = new ExtendedSearchGinArrayDirectFilter
            {
                TempStoragePool = _tempStoragePool,
                CommonIndex = invertedIndex,
                RelevanceFilter = _relevanceFilter,
                PositionSearchType = PositionSearchType.BinarySearch
            };

            extendedSearchGinArrayDirectFilterBs.FindExtended(searchVector, metricsCalculator, cancellationToken);
        }
    }*/

    private void RunDirectHashSearch(TokenVector searchVector, IMetricsCalculator metricsCalculator,
        CancellationToken cancellationToken)
    {
        foreach (var invertedIndexHs in _partitionsHs.Indices)
        {
            var extendedSearchGinArrayDirectHs = new ExtendedSearchGinArrayDirect
            {
                TempStoragePool = _tempStoragePool,
                CommonIndex = invertedIndexHs,
                PositionSearchType = PositionSearchType.LinearScan
            };

            extendedSearchGinArrayDirectHs.FindExtended(searchVector, metricsCalculator, cancellationToken);
        }
    }

    /*private void RunDirectFilterHashSearch(TokenVector searchVector, IMetricsCalculator metricsCalculator,
        CancellationToken cancellationToken)
    {
        foreach (var invertedIndexHs in _partitionsHs.Indices)
        {
            var extendedSearchGinArrayDirectFilterHs = new ExtendedSearchGinArrayDirectFilter
            {
                TempStoragePool = _tempStoragePool,
                CommonIndex = invertedIndexHs,
                RelevanceFilter = _relevanceFilter,
                PositionSearchType = PositionSearchType.LinearScan
            };

            extendedSearchGinArrayDirectFilterHs.FindExtended(searchVector, metricsCalculator, cancellationToken);
        }
    }*/
}
