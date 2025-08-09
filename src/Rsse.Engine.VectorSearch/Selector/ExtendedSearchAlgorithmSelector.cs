using System;
using RsseEngine.Algorithms;
using RsseEngine.Contracts;
using RsseEngine.Dto;
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
    /// <summary>
    /// Поддержка инвертированного индекса для расширенного поиска и метрик.
    /// </summary>
    private readonly InvertedIndex<DocumentIdList> _invertedIndex = new();

    private readonly InvertedOffsetIndex _invertedOffsetIndex = new();

    private readonly DirectOffsetIndex _directOffsetIndex = new();

    private readonly FrozenDirectOffsetIndex _frozenDirectOffsetIndex = new();

    private readonly ArrayDirectOffsetIndex _arrayDirectOffsetIndex = new();

    private readonly ExtendedSearchLegacy _extendedSearchLegacy;
    private readonly ExtendedSearchGinFilter _extendedSearchGinFilter;
    private readonly IExtendedSearchProcessor _extendedSearchGinMerge;
    private readonly IExtendedSearchProcessor _extendedSearchGinMergeFilter;
    private readonly ExtendedSearchGinOffset _extendedSearchGinOffset;
    private readonly ExtendedSearchGinOffsetFilter _extendedSearchGinOffsetFilter;
    private readonly ExtendedSearchGinDirectOffset _extendedSearchGinDirectOffsetLs;
    private readonly ExtendedSearchGinDirectOffsetFilter _extendedSearchGinDirectOffsetFilterLs;
    private readonly ExtendedSearchGinDirectOffset _extendedSearchGinDirectOffsetBs;
    private readonly ExtendedSearchGinDirectOffsetFilter _extendedSearchGinDirectOffsetFilterBs;
    private readonly ExtendedSearchGinFrozenDirect _extendedSearchGinFrozenDirect;
    private readonly ExtendedSearchGinFrozenDirectFilter _extendedSearchGinFrozenDirectFilter;
    private readonly ExtendedSearchGinArrayDirect _extendedSearchGinArrayDirectLs;
    private readonly ExtendedSearchGinArrayDirectFilter _extendedSearchGinArrayDirectFilterLs;
    private readonly ExtendedSearchGinArrayDirect _extendedSearchGinArrayDirectBs;
    private readonly ExtendedSearchGinArrayDirectFilter _extendedSearchGinArrayDirectFilterBs;

    /// <summary>
    /// Компонент с extended-алгоритмами.
    /// </summary>
    /// <param name="tempStoragePool">Пул коллекций.</param>
    /// <param name="generalDirectIndex">Общий индекс.</param>
    /// <param name="relevancyThreshold">Порог релевантности.</param>
    public ExtendedSearchAlgorithmSelector(TempStoragePool tempStoragePool,
        DirectIndex generalDirectIndex, double relevancyThreshold)
    {
        var relevanceFilter = new GinRelevanceFilter
        {
            Threshold = relevancyThreshold
        };

        // Без GIN-индекса.
        _extendedSearchLegacy = new ExtendedSearchLegacy
        {
            GeneralDirectIndex = generalDirectIndex
        };

        _extendedSearchGinFilter = new ExtendedSearchGinFilter
        {
            TempStoragePool = tempStoragePool,
            GeneralDirectIndex = generalDirectIndex,
            GinExtended = _invertedIndex,
            RelevanceFilter = relevanceFilter
        };

        _extendedSearchGinMerge = new ExtendedSearchGinMerge
        {
            TempStoragePool = tempStoragePool,
            GeneralDirectIndex = generalDirectIndex,
            GinExtended = _invertedIndex
        };

        _extendedSearchGinMergeFilter = new ExtendedSearchGinMergeFilter
        {
            TempStoragePool = tempStoragePool,
            GeneralDirectIndex = generalDirectIndex,
            GinExtended = _invertedIndex,
            RelevanceFilter = relevanceFilter
        };

        _extendedSearchGinOffset = new ExtendedSearchGinOffset
        {
            TempStoragePool = tempStoragePool,
            GinExtended = _invertedOffsetIndex
        };

        _extendedSearchGinOffsetFilter = new ExtendedSearchGinOffsetFilter
        {
            TempStoragePool = tempStoragePool,
            GinExtended = _invertedOffsetIndex,
            RelevanceFilter = relevanceFilter
        };

        _extendedSearchGinDirectOffsetLs = new ExtendedSearchGinDirectOffset
        {
            TempStoragePool = tempStoragePool,
            GinExtended = _directOffsetIndex,
            PositionSearchType = PositionSearchType.LinearScan
        };

        _extendedSearchGinDirectOffsetFilterLs = new ExtendedSearchGinDirectOffsetFilter
        {
            TempStoragePool = tempStoragePool,
            GinExtended = _directOffsetIndex,
            RelevanceFilter = relevanceFilter,
            PositionSearchType = PositionSearchType.LinearScan
        };

        _extendedSearchGinDirectOffsetBs = new ExtendedSearchGinDirectOffset
        {
            TempStoragePool = tempStoragePool,
            GinExtended = _directOffsetIndex,
            PositionSearchType = PositionSearchType.BinarySearch
        };

        _extendedSearchGinDirectOffsetFilterBs = new ExtendedSearchGinDirectOffsetFilter
        {
            TempStoragePool = tempStoragePool,
            GinExtended = _directOffsetIndex,
            RelevanceFilter = relevanceFilter,
            PositionSearchType = PositionSearchType.BinarySearch
        };

        _extendedSearchGinFrozenDirect = new ExtendedSearchGinFrozenDirect
        {
            TempStoragePool = tempStoragePool,
            GinExtended = _frozenDirectOffsetIndex
        };

        _extendedSearchGinFrozenDirectFilter = new ExtendedSearchGinFrozenDirectFilter
        {
            TempStoragePool = tempStoragePool,
            GinExtended = _frozenDirectOffsetIndex,
            RelevanceFilter = relevanceFilter
        };

        _extendedSearchGinArrayDirectLs = new ExtendedSearchGinArrayDirect
        {
            TempStoragePool = tempStoragePool,
            GinExtended = _arrayDirectOffsetIndex,
            PositionSearchType = PositionSearchType.LinearScan
        };

        _extendedSearchGinArrayDirectFilterLs = new ExtendedSearchGinArrayDirectFilter
        {
            TempStoragePool = tempStoragePool,
            GinExtended = _arrayDirectOffsetIndex,
            RelevanceFilter = relevanceFilter,
            PositionSearchType = PositionSearchType.LinearScan
        };

        _extendedSearchGinArrayDirectBs = new ExtendedSearchGinArrayDirect
        {
            TempStoragePool = tempStoragePool,
            GinExtended = _arrayDirectOffsetIndex,
            PositionSearchType = PositionSearchType.BinarySearch
        };

        _extendedSearchGinArrayDirectFilterBs = new ExtendedSearchGinArrayDirectFilter
        {
            TempStoragePool = tempStoragePool,
            GinExtended = _arrayDirectOffsetIndex,
            RelevanceFilter = relevanceFilter,
            PositionSearchType = PositionSearchType.BinarySearch
        };
    }

    /// <inheritdoc/>
    public IExtendedSearchProcessor GetSearchProcessor(ExtendedSearchType searchType)
    {
        return searchType switch
        {
            ExtendedSearchType.Legacy => _extendedSearchLegacy,
            ExtendedSearchType.GinFilter => _extendedSearchGinFilter,
            ExtendedSearchType.GinMerge => _extendedSearchGinMerge,
            ExtendedSearchType.GinMergeFilter => _extendedSearchGinMergeFilter,
            ExtendedSearchType.GinOffset => _extendedSearchGinOffset,
            ExtendedSearchType.GinOffsetFilter => _extendedSearchGinOffsetFilter,
            ExtendedSearchType.GinDirectOffsetLs => _extendedSearchGinDirectOffsetLs,
            ExtendedSearchType.GinDirectOffsetFilterLs => _extendedSearchGinDirectOffsetFilterLs,
            ExtendedSearchType.GinDirectOffsetBs => _extendedSearchGinDirectOffsetBs,
            ExtendedSearchType.GinDirectOffsetFilterBs => _extendedSearchGinDirectOffsetFilterBs,
            ExtendedSearchType.GinFrozenDirect => _extendedSearchGinFrozenDirect,
            ExtendedSearchType.GinFrozenDirectFilter => _extendedSearchGinFrozenDirectFilter,
            ExtendedSearchType.GinArrayDirectLs => _extendedSearchGinArrayDirectLs,
            ExtendedSearchType.GinArrayDirectFilterLs => _extendedSearchGinArrayDirectFilterLs,
            ExtendedSearchType.GinArrayDirectBs => _extendedSearchGinArrayDirectBs,
            ExtendedSearchType.GinArrayDirectFilterBs => _extendedSearchGinArrayDirectFilterBs,
            _ => throw new ArgumentOutOfRangeException(nameof(searchType), searchType,
                "unknown search type")
        };
    }

    /// <inheritdoc/>
    public void AddVector(DocumentId documentId, TokenVector tokenVector)
    {
        _invertedIndex.AddVector(documentId, tokenVector);
        _invertedOffsetIndex.AddOrUpdateVector(documentId, tokenVector);
        _directOffsetIndex.AddOrUpdateVector(documentId, tokenVector);
        _frozenDirectOffsetIndex.AddOrUpdateVector(documentId, tokenVector);
        _arrayDirectOffsetIndex.AddOrUpdateVector(documentId, tokenVector);
    }

    /// <inheritdoc/>
    public void UpdateVector(DocumentId documentId, TokenVector tokenVector, TokenVector oldTokenVector)
    {
        _invertedIndex.UpdateVector(documentId, tokenVector, oldTokenVector);
        _invertedOffsetIndex.AddOrUpdateVector(documentId, tokenVector);
        _directOffsetIndex.AddOrUpdateVector(documentId, tokenVector);
        _frozenDirectOffsetIndex.AddOrUpdateVector(documentId, tokenVector);
        _arrayDirectOffsetIndex.AddOrUpdateVector(documentId, tokenVector);
    }

    /// <inheritdoc/>
    public void RemoveVector(DocumentId documentId, TokenVector tokenVector)
    {
        _invertedIndex.RemoveVector(documentId, tokenVector);
        _invertedOffsetIndex.RemoveVector(documentId);
        _directOffsetIndex.RemoveVector(documentId);
        _frozenDirectOffsetIndex.RemoveVector(documentId);
        _arrayDirectOffsetIndex.RemoveVector(documentId);
    }

    /// <inheritdoc/>
    public void Clear()
    {
        _invertedIndex.Clear();
        _invertedOffsetIndex.Clear();
        _directOffsetIndex.Clear();
        _frozenDirectOffsetIndex.Clear();
        _arrayDirectOffsetIndex.Clear();
    }
}
