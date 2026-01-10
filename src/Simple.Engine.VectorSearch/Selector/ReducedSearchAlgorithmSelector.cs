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
/// Компонент, предоставляющий доступ к различным алгоритмам reduced-поиска.
/// </summary>
[Obsolete("R&D only")]
public sealed class ReducedSearchAlgorithmSelector : ISearchAlgorithmSelector<ReducedSearchType, IReducedSearchProcessor>
{
    private readonly TempStoragePool _tempStoragePool;

    private readonly GeneralDirectIndexLegacy _generalDirectIndexLegacy;

    private readonly CommonIndices _partitions = new(IndexPoint.DictionaryStorageType.SortedArrayStorage);

    private readonly InvertedIndexLegacy _invertedIndexLegacy = new();

    /// <summary>
    /// Компонент с reduced-алгоритмами.
    /// </summary>
    /// <param name="tempStoragePool">Пул коллекций.</param>
    /// <param name="generalDirectIndexLegacy">Общий индекс, используется в legacy-алгоритме.</param>
    /// <param name="relevancyThreshold">Порог релевантности</param>
    public ReducedSearchAlgorithmSelector(TempStoragePool tempStoragePool,
        GeneralDirectIndexLegacy generalDirectIndexLegacy, double relevancyThreshold)
    {
        // защита на случай изменения внешних проверок, до момента готовности алгоритмов
        EnvironmentReporter.ThrowIfProduction(nameof(ReducedSearchAlgorithmSelector));

        _tempStoragePool = tempStoragePool;
        _generalDirectIndexLegacy = generalDirectIndexLegacy;
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
            case ReducedSearchType.SimpleLegacy:
                {
                    RunSimpleLegacySearch(searchVector, metricsCalculator, cancellationToken);
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

        _invertedIndexLegacy.TryAdd(documentId, tokenVector);
    }

    /// <inheritdoc/>
    public void UpdateVector(DocumentId documentId, TokenVector tokenVector)
    {
        _partitions.AddOrUpdateVector(documentId, tokenVector);

        var oldTokenLine = _generalDirectIndexLegacy[documentId];
        _invertedIndexLegacy.TryUpdate(documentId, tokenVector, oldTokenLine.Reduced);
    }

    /// <inheritdoc/>
    public void RemoveVector(DocumentId documentId, TokenVector tokenVector)
    {
        _partitions.RemoveVector(documentId);

        _invertedIndexLegacy.TryRemoveDocumentId(documentId, tokenVector);
    }

    /// <inheritdoc/>
    public void Clear()
    {
        _partitions.Clear();

        _invertedIndexLegacy.Clear();
    }

    private void RunLegacySearch(TokenVector searchVector, IMetricsCalculator metricsCalculator,
        CancellationToken cancellationToken)
    {
        var reducedSearchLegacy = new ReducedSearchLegacy
        {
            GeneralDirectIndexLegacy = _generalDirectIndexLegacy
        };

        reducedSearchLegacy.FindReduced(searchVector, metricsCalculator, cancellationToken);
    }

    private void RunSimpleLegacySearch(TokenVector searchVector, IMetricsCalculator metricsCalculator,
        CancellationToken cancellationToken)
    {
        var reducedSearchLegacy = new ReducedSearchSimple
        {
            GeneralDirectIndexLegacy = _generalDirectIndexLegacy,
            InvertedIndexLegacy = _invertedIndexLegacy,
            TempStoragePool = _tempStoragePool
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
}
