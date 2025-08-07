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
public sealed class ExtendedSearchAlgorithmSelector<TDocumentIdCollection>
    : ISearchAlgorithmSelector<ExtendedSearchType, IExtendedSearchProcessor>
    where TDocumentIdCollection : struct, IDocumentIdCollection<TDocumentIdCollection>
{
    /// <summary>
    /// Поддержка инвертированного индекса для расширенного поиска и метрик.
    /// </summary>
    private readonly InvertedIndex<TDocumentIdCollection> _invertedIndex = new();

    private readonly InvertedOffsetIndex _invertedOffsetIndex = new();

    private readonly DirectOffsetIndex _directOffsetIndex = new();

    private readonly FrozenDirectOffsetIndex _frozenDirectOffsetIndex = new();

    private readonly ExtendedSearchLegacy _extendedSearchLegacy;
    private readonly ExtendedSearchGinFilter<TDocumentIdCollection> _extendedSearchGinFilter;
    private readonly ExtendedSearchGinFast<TDocumentIdCollection> _extendedSearchGinFast;
    private readonly ExtendedSearchGinFastFilter<TDocumentIdCollection> _extendedSearchGinFastFilter;
    private readonly IExtendedSearchProcessor _extendedSearchGinMerge;
    private readonly IExtendedSearchProcessor _extendedSearchGinMergeFilter;
    private readonly ExtendedSearchGinOffset _extendedSearchGinOffset;
    private readonly ExtendedSearchGinOffsetFilter _extendedSearchGinOffsetFilter;
    private readonly ExtendedSearchGinDirectOffset _extendedSearchGinDirectOffset;
    private readonly ExtendedSearchGinDirectOffsetFilter _extendedSearchGinDirectOffsetFilter;
    private readonly ExtendedSearchGinFrozenDirect _extendedSearchGinFrozenDirect;
    private readonly ExtendedSearchGinFrozenDirectFilter _extendedSearchGinFrozenDirectFilter;

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

        _extendedSearchGinFilter = new ExtendedSearchGinFilter<TDocumentIdCollection>
        {
            TempStoragePool = tempStoragePool,
            GeneralDirectIndex = generalDirectIndex,
            GinExtended = _invertedIndex,
            RelevanceFilter = relevanceFilter
        };

        // С GIN-индексом.
        _extendedSearchGinFast = new ExtendedSearchGinFast<TDocumentIdCollection>
        {
            TempStoragePool = tempStoragePool,
            GeneralDirectIndex = generalDirectIndex,
            GinExtended = _invertedIndex
        };

        _extendedSearchGinFastFilter = new ExtendedSearchGinFastFilter<TDocumentIdCollection>
        {
            TempStoragePool = tempStoragePool,
            GeneralDirectIndex = generalDirectIndex,
            GinExtended = _invertedIndex,
            RelevanceFilter = relevanceFilter
        };

        if (typeof(TDocumentIdCollection) == typeof(DocumentIdList))
        {
            _extendedSearchGinMerge = new ExtendedSearchGinMerge
            {
                TempStoragePool = tempStoragePool,
                GeneralDirectIndex = generalDirectIndex,
                GinExtended = (InvertedIndex<DocumentIdList>)(object)_invertedIndex
            };

            _extendedSearchGinMergeFilter = new ExtendedSearchGinMergeFilter
            {
                TempStoragePool = tempStoragePool,
                GeneralDirectIndex = generalDirectIndex,
                GinExtended = (InvertedIndex<DocumentIdList>)(object)_invertedIndex,
                RelevanceFilter = relevanceFilter
            };
        }
        else if (typeof(TDocumentIdCollection) == typeof(DocumentIdSet))
        {
            // Fallback для DocumentIdSet
            _extendedSearchGinMerge = _extendedSearchGinFastFilter;
            _extendedSearchGinMergeFilter = _extendedSearchGinFastFilter;
        }
        else
        {
            throw new NotSupportedException($"[{nameof(TDocumentIdCollection)}] is not supported.");
        }

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

        _extendedSearchGinDirectOffset = new ExtendedSearchGinDirectOffset
        {
            TempStoragePool = tempStoragePool,
            GinExtended = _directOffsetIndex
        };

        _extendedSearchGinDirectOffsetFilter = new ExtendedSearchGinDirectOffsetFilter
        {
            TempStoragePool = tempStoragePool,
            GinExtended = _directOffsetIndex,
            RelevanceFilter = relevanceFilter
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
    }

    /// <inheritdoc/>
    public IExtendedSearchProcessor GetSearchProcessor(ExtendedSearchType searchType)
    {
        return searchType switch
        {
            ExtendedSearchType.Legacy => _extendedSearchLegacy,
            ExtendedSearchType.GinFilter => _extendedSearchGinFilter,
            ExtendedSearchType.GinFast => _extendedSearchGinFast,
            ExtendedSearchType.GinFastFilter => _extendedSearchGinFastFilter,
            ExtendedSearchType.GinMerge => _extendedSearchGinMerge,
            ExtendedSearchType.GinMergeFilter => _extendedSearchGinMergeFilter,
            ExtendedSearchType.GinOffset => _extendedSearchGinOffset,
            ExtendedSearchType.GinOffsetFilter => _extendedSearchGinOffsetFilter,
            ExtendedSearchType.GinDirectOffset => _extendedSearchGinDirectOffset,
            ExtendedSearchType.GinDirectOffsetFilter => _extendedSearchGinDirectOffsetFilter,
            ExtendedSearchType.GinFrozenDirect => _extendedSearchGinFrozenDirect,
            ExtendedSearchType.GinFrozenDirectFilter => _extendedSearchGinFrozenDirectFilter,
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
    }

    /// <inheritdoc/>
    public void UpdateVector(DocumentId documentId, TokenVector tokenVector, TokenVector oldTokenVector)
    {
        _invertedIndex.UpdateVector(documentId, tokenVector, oldTokenVector);
        _invertedOffsetIndex.AddOrUpdateVector(documentId, tokenVector);
        _directOffsetIndex.AddOrUpdateVector(documentId, tokenVector);
        _frozenDirectOffsetIndex.AddOrUpdateVector(documentId, tokenVector);
    }

    /// <inheritdoc/>
    public void RemoveVector(DocumentId documentId, TokenVector tokenVector)
    {
        _invertedIndex.RemoveVector(documentId, tokenVector);
        _invertedOffsetIndex.RemoveVector(documentId);
        _directOffsetIndex.RemoveVector(documentId);
        _frozenDirectOffsetIndex.RemoveVector(documentId);
    }

    /// <inheritdoc/>
    public void Clear()
    {
        _invertedIndex.Clear();
        _invertedOffsetIndex.Clear();
        _directOffsetIndex.Clear();
        _frozenDirectOffsetIndex.Clear();
    }
}
