using System;
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
public sealed class ReducedSearchAlgorithmSelector<TDocumentIdCollection>
    : ISearchAlgorithmSelector<ReducedSearchType, IReducedSearchProcessor>
    where TDocumentIdCollection : struct, IDocumentIdCollection<TDocumentIdCollection>
{
    /// <summary>
    /// Поддержка инвертированного индекса для сокращенного поиска и метрик.
    /// </summary>
    private readonly InvertedIndex<TDocumentIdCollection> _ginReduced = new();

    private readonly ArrayDirectOffsetIndex _arrayDirectOffsetIndex = new(DocumentDataPoint.DocumentDataPointSearchType.BinaryTree);

    private readonly ArrayDirectOffsetIndex _arrayDirectOffsetIndexHs = new(DocumentDataPoint.DocumentDataPointSearchType.HashMap);

    private readonly ReducedSearchLegacy _reducedSearchLegacy;
    private readonly ReducedSearchGinOptimized<TDocumentIdCollection> _reducedSearchGinOptimized;
    private readonly ReducedSearchGinOptimizedFilter<TDocumentIdCollection> _reducedSearchGinOptimizedFilter;
    private readonly ReducedSearchGinFast<TDocumentIdCollection> _reducedSearchGinFast;
    private readonly ReducedSearchGinFastFilter<TDocumentIdCollection> _reducedSearchGinFastFilter;
    private readonly IReducedSearchProcessor _reducedSearchGinMerge;
    private readonly IReducedSearchProcessor _reducedSearchGinMergeFilter;
    private readonly IReducedSearchProcessor _reducedSearchGinArrayDirect;
    private readonly IReducedSearchProcessor _reducedSearchGinArrayMergeFilter;
    private readonly IReducedSearchProcessor _reducedSearchGinArrayDirectFilterLs;
    private readonly IReducedSearchProcessor _reducedSearchGinArrayDirectFilterBs;
    private readonly IReducedSearchProcessor _reducedSearchGinArrayDirectFilterHs;

    /// <summary>
    /// Компонент с reduced-алгоритмами.
    /// </summary>
    /// <param name="tempStoragePool">Пул коллекций.</param>
    /// <param name="generalDirectIndex">Общий индекс.</param>
    /// <param name="relevancyThreshold">Порог релевантности</param>
    public ReducedSearchAlgorithmSelector(TempStoragePool tempStoragePool,
        DirectIndex generalDirectIndex, double relevancyThreshold)
    {
        var relevanceFilter = new GinRelevanceFilter
        {
            Threshold = relevancyThreshold
        };

        // Без GIN-индекса.
        _reducedSearchLegacy = new ReducedSearchLegacy
        {
            GeneralDirectIndex = generalDirectIndex
        };

        // С GIN-индексом.
        _reducedSearchGinOptimized = new ReducedSearchGinOptimized<TDocumentIdCollection>
        {
            TempStoragePool = tempStoragePool,
            GeneralDirectIndex = generalDirectIndex,
            GinReduced = _ginReduced
        };

        // С GIN-индексом.
        _reducedSearchGinOptimizedFilter = new ReducedSearchGinOptimizedFilter<TDocumentIdCollection>
        {
            TempStoragePool = tempStoragePool,
            GeneralDirectIndex = generalDirectIndex,
            GinReduced = _ginReduced,
            RelevanceFilter = relevanceFilter
        };

        // С GIN-индексом.
        _reducedSearchGinFast = new ReducedSearchGinFast<TDocumentIdCollection>
        {
            TempStoragePool = tempStoragePool,
            GeneralDirectIndex = generalDirectIndex,
            GinReduced = _ginReduced
        };

        // С GIN-индексом.
        _reducedSearchGinFastFilter = new ReducedSearchGinFastFilter<TDocumentIdCollection>
        {
            TempStoragePool = tempStoragePool,
            GeneralDirectIndex = generalDirectIndex,
            GinReduced = _ginReduced,
            RelevanceFilter = relevanceFilter
        };

        if (typeof(TDocumentIdCollection) == typeof(DocumentIdList))
        {
            _reducedSearchGinMerge = new ReducedSearchGinMerge
            {
                TempStoragePool = tempStoragePool,
                GeneralDirectIndex = generalDirectIndex,
                GinReduced = (InvertedIndex<DocumentIdList>)(object)_ginReduced
            };

            _reducedSearchGinMergeFilter = new ReducedSearchGinMergeFilter
            {
                TempStoragePool = tempStoragePool,
                GeneralDirectIndex = generalDirectIndex,
                GinReduced = (InvertedIndex<DocumentIdList>)(object)_ginReduced,
                RelevanceFilter = relevanceFilter
            };

            _reducedSearchGinArrayDirect = new ReducedSearchGinArrayDirect
            {
                TempStoragePool = tempStoragePool,
                GinReduced = _arrayDirectOffsetIndex,
            };

            _reducedSearchGinArrayMergeFilter = new ReducedSearchGinArrayMergeFilter
            {
                TempStoragePool = tempStoragePool,
                GinReduced = _arrayDirectOffsetIndex,
                RelevanceFilter = relevanceFilter
            };

            _reducedSearchGinArrayDirectFilterLs = new ReducedSearchGinArrayDirectFilter
            {
                TempStoragePool = tempStoragePool,
                GinReduced = _arrayDirectOffsetIndex,
                RelevanceFilter = relevanceFilter,
                PositionSearchType = PositionSearchType.LinearScan
            };

            _reducedSearchGinArrayDirectFilterBs = new ReducedSearchGinArrayDirectFilter
            {
                TempStoragePool = tempStoragePool,
                GinReduced = _arrayDirectOffsetIndex,
                RelevanceFilter = relevanceFilter,
                PositionSearchType = PositionSearchType.BinarySearch
            };

            _reducedSearchGinArrayDirectFilterHs = new ReducedSearchGinArrayDirectFilter
            {
                TempStoragePool = tempStoragePool,
                GinReduced = _arrayDirectOffsetIndexHs,
                RelevanceFilter = relevanceFilter,
                PositionSearchType = PositionSearchType.LinearScan
            };
        }
        else if (typeof(TDocumentIdCollection) == typeof(DocumentIdSet))
        {
            // Fallback для DocumentIdSet
            _reducedSearchGinMerge = _reducedSearchGinOptimizedFilter;
            _reducedSearchGinMergeFilter = _reducedSearchGinOptimizedFilter;
            _reducedSearchGinArrayDirect = _reducedSearchGinOptimizedFilter;
            _reducedSearchGinArrayMergeFilter = _reducedSearchGinOptimizedFilter;
            _reducedSearchGinArrayDirectFilterLs = _reducedSearchGinOptimizedFilter;
            _reducedSearchGinArrayDirectFilterBs = _reducedSearchGinOptimizedFilter;
            _reducedSearchGinArrayDirectFilterHs = _reducedSearchGinOptimizedFilter;
        }
        else
        {
            throw new NotSupportedException($"[{nameof(TDocumentIdCollection)}] is not supported.");
        }
    }

    /// <inheritdoc/>
    public IReducedSearchProcessor GetSearchProcessor(ReducedSearchType searchType)
    {
        return searchType switch
        {
            ReducedSearchType.Legacy => _reducedSearchLegacy,
            ReducedSearchType.GinOptimized => _reducedSearchGinOptimized,
            ReducedSearchType.GinOptimizedFilter => _reducedSearchGinOptimizedFilter,
            ReducedSearchType.GinFast => _reducedSearchGinFast,
            ReducedSearchType.GinFastFilter => _reducedSearchGinFastFilter,
            ReducedSearchType.GinMerge => _reducedSearchGinMerge,
            ReducedSearchType.GinMergeFilter => _reducedSearchGinMergeFilter,
            ReducedSearchType.GinArrayDirect => _reducedSearchGinArrayDirect,
            ReducedSearchType.GinArrayMergeFilter => _reducedSearchGinArrayMergeFilter,
            ReducedSearchType.GinArrayDirectFilterLs => _reducedSearchGinArrayDirectFilterLs,
            ReducedSearchType.GinArrayDirectFilterBs => _reducedSearchGinArrayDirectFilterBs,
            ReducedSearchType.GinArrayDirectFilterHs => _reducedSearchGinArrayDirectFilterHs,
            _ => throw new ArgumentOutOfRangeException(nameof(searchType), searchType,
                "unknown search type")
        };
    }

    /// <inheritdoc/>
    public void AddVector(DocumentId documentId, TokenVector tokenVector)
    {
        _ginReduced.AddVector(documentId, tokenVector);
        _arrayDirectOffsetIndex.AddOrUpdateVector(documentId, tokenVector);
        _arrayDirectOffsetIndexHs.AddOrUpdateVector(documentId, tokenVector);
    }

    /// <inheritdoc/>
    public void UpdateVector(DocumentId documentId, TokenVector tokenVector, TokenVector oldTokenVector)
    {
        _ginReduced.UpdateVector(documentId, tokenVector, oldTokenVector);
        _arrayDirectOffsetIndex.AddOrUpdateVector(documentId, tokenVector);
        _arrayDirectOffsetIndexHs.AddOrUpdateVector(documentId, tokenVector);
    }

    /// <inheritdoc/>
    public void RemoveVector(DocumentId documentId, TokenVector tokenVector)
    {
        _ginReduced.RemoveVector(documentId, tokenVector);
        _arrayDirectOffsetIndex.RemoveVector(documentId);
        _arrayDirectOffsetIndexHs.RemoveVector(documentId);
    }

    /// <inheritdoc/>
    public void Clear()
    {
        _ginReduced.Clear();
        _arrayDirectOffsetIndex.Clear();
        _arrayDirectOffsetIndexHs.Clear();
    }
}
