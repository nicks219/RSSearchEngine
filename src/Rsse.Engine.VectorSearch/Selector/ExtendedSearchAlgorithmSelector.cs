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
/// Компонент, предоставляющий доступ к различным алгоритмам extended-поиска.
/// </summary>
public sealed class ExtendedSearchAlgorithmSelector
    : ISearchAlgorithmSelector<ExtendedSearchType, IExtendedSearchProcessor>
{
    /// <summary>
    /// Поддержка инвертированного индекса для расширенного поиска и метрик.
    /// </summary>
    private readonly InvertedOffsetIndex _invertedOffsetIndex = new();

    private readonly ArrayDirectOffsetIndex _arrayDirectOffsetIndex = new(DocumentDataPoint.DocumentDataPointSearchType.BinaryTree);

    private readonly ArrayDirectOffsetIndex _arrayDirectOffsetIndexHs = new(DocumentDataPoint.DocumentDataPointSearchType.HashMap);

    private readonly ExtendedSearchLegacy _extendedSearchLegacy;
    private readonly ExtendedSearchGinOffset _extendedSearchGinOffset;
    private readonly ExtendedSearchGinOffsetFilter _extendedSearchGinOffsetFilter;
    private readonly ExtendedSearchGinArrayDirect _extendedSearchGinArrayDirectLs;
    private readonly ExtendedSearchGinArrayDirectFilter _extendedSearchGinArrayDirectFilterLs;
    private readonly ExtendedSearchGinArrayDirect _extendedSearchGinArrayDirectBs;
    private readonly ExtendedSearchGinArrayDirectFilter _extendedSearchGinArrayDirectFilterBs;
    private readonly ExtendedSearchGinArrayDirect _extendedSearchGinArrayDirectHs;
    private readonly ExtendedSearchGinArrayDirectFilter _extendedSearchGinArrayDirectFilterHs;

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

        _extendedSearchGinArrayDirectHs = new ExtendedSearchGinArrayDirect
        {
            TempStoragePool = tempStoragePool,
            GinExtended = _arrayDirectOffsetIndexHs,
            PositionSearchType = PositionSearchType.LinearScan
        };

        _extendedSearchGinArrayDirectFilterHs = new ExtendedSearchGinArrayDirectFilter
        {
            TempStoragePool = tempStoragePool,
            GinExtended = _arrayDirectOffsetIndexHs,
            RelevanceFilter = relevanceFilter,
            PositionSearchType = PositionSearchType.LinearScan
        };
    }

    /// <inheritdoc/>
    public IExtendedSearchProcessor GetSearchProcessor(ExtendedSearchType searchType)
    {
        return searchType switch
        {
            ExtendedSearchType.Legacy => _extendedSearchLegacy,
            ExtendedSearchType.GinOffset => _extendedSearchGinOffset,
            ExtendedSearchType.GinOffsetFilter => _extendedSearchGinOffsetFilter,
            ExtendedSearchType.GinArrayDirectLs => _extendedSearchGinArrayDirectLs,
            ExtendedSearchType.GinArrayDirectFilterLs => _extendedSearchGinArrayDirectFilterLs,
            ExtendedSearchType.GinArrayDirectBs => _extendedSearchGinArrayDirectBs,
            ExtendedSearchType.GinArrayDirectFilterBs => _extendedSearchGinArrayDirectFilterBs,
            ExtendedSearchType.GinArrayDirectHs => _extendedSearchGinArrayDirectHs,
            ExtendedSearchType.GinArrayDirectFilterHs => _extendedSearchGinArrayDirectFilterHs,
            _ => throw new ArgumentOutOfRangeException(nameof(searchType), searchType,
                "unknown search type")
        };
    }

    /// <inheritdoc/>
    public void AddVector(DocumentId documentId, TokenVector tokenVector)
    {
        _invertedOffsetIndex.AddOrUpdateVector(documentId, tokenVector);
        _arrayDirectOffsetIndex.AddOrUpdateVector(documentId, tokenVector);
        _arrayDirectOffsetIndexHs.AddOrUpdateVector(documentId, tokenVector);
    }

    /// <inheritdoc/>
    public void UpdateVector(DocumentId documentId, TokenVector tokenVector, TokenVector oldTokenVector)
    {
        _invertedOffsetIndex.AddOrUpdateVector(documentId, tokenVector);
        _arrayDirectOffsetIndex.AddOrUpdateVector(documentId, tokenVector);
        _arrayDirectOffsetIndexHs.AddOrUpdateVector(documentId, tokenVector);
    }

    /// <inheritdoc/>
    public void RemoveVector(DocumentId documentId, TokenVector tokenVector)
    {
        _invertedOffsetIndex.RemoveVector(documentId);
        _arrayDirectOffsetIndex.RemoveVector(documentId);
        _arrayDirectOffsetIndexHs.RemoveVector(documentId);
    }

    /// <inheritdoc/>
    public void Clear()
    {
        _invertedOffsetIndex.Clear();
        _arrayDirectOffsetIndex.Clear();
        _arrayDirectOffsetIndexHs.Clear();
    }
}
