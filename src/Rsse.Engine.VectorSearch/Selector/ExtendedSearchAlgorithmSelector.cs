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
    private readonly InvertedIndex<TDocumentIdCollection> _ginExtended = new();

    private readonly InvertedOffsetIndex _ginOffsetExtended = new();

    private readonly ExtendedSearchLegacy _extendedSearchLegacy;
    private readonly ExtendedSearchGinSimple<TDocumentIdCollection> _extendedSearchGinSimple;
    private readonly ExtendedSearchGinOptimized<TDocumentIdCollection> _extendedSearchGinOptimized;
    private readonly ExtendedSearchGinFilter<TDocumentIdCollection> _extendedSearchGinFilter;
    private readonly ExtendedSearchGinFast<TDocumentIdCollection> _extendedSearchGinFast;
    private readonly ExtendedSearchGinFastFilter<TDocumentIdCollection> _extendedSearchGinFastFilter;
    private readonly IExtendedSearchProcessor _extendedSearchGinMerge;
    private readonly IExtendedSearchProcessor _extendedSearchGinMergeFilter;
    private readonly ExtendedSearchGinOffset _extendedSearchGinOffset;
    private readonly ExtendedSearchGinOffsetFilter _extendedSearchGinOffsetFilter;

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

        // С GIN-индексом.
        _extendedSearchGinSimple = new ExtendedSearchGinSimple<TDocumentIdCollection>
        {
            GeneralDirectIndex = generalDirectIndex,
            GinExtended = _ginExtended
        };

        // С GIN-индексом.
        _extendedSearchGinOptimized = new ExtendedSearchGinOptimized<TDocumentIdCollection>
        {
            TempStoragePool = tempStoragePool,
            GeneralDirectIndex = generalDirectIndex,
            GinExtended = _ginExtended
        };

        _extendedSearchGinFilter = new ExtendedSearchGinFilter<TDocumentIdCollection>
        {
            TempStoragePool = tempStoragePool,
            GeneralDirectIndex = generalDirectIndex,
            GinExtended = _ginExtended,
            RelevanceFilter = relevanceFilter
        };

        // С GIN-индексом.
        _extendedSearchGinFast = new ExtendedSearchGinFast<TDocumentIdCollection>
        {
            TempStoragePool = tempStoragePool,
            GeneralDirectIndex = generalDirectIndex,
            GinExtended = _ginExtended
        };

        _extendedSearchGinFastFilter = new ExtendedSearchGinFastFilter<TDocumentIdCollection>
        {
            TempStoragePool = tempStoragePool,
            GeneralDirectIndex = generalDirectIndex,
            GinExtended = _ginExtended,
            RelevanceFilter = relevanceFilter
        };

        if (typeof(TDocumentIdCollection) == typeof(DocumentIdList))
        {
            _extendedSearchGinMerge = new ExtendedSearchGinMerge
            {
                TempStoragePool = tempStoragePool,
                GeneralDirectIndex = generalDirectIndex,
                GinExtended = (InvertedIndex<DocumentIdList>)(object)_ginExtended
            };

            _extendedSearchGinMergeFilter = new ExtendedSearchGinMergeFilter
            {
                TempStoragePool = tempStoragePool,
                GeneralDirectIndex = generalDirectIndex,
                GinExtended = (InvertedIndex<DocumentIdList>)(object)_ginExtended,
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
            GeneralDirectIndex = generalDirectIndex,
            GinExtended = _ginOffsetExtended
        };

        _extendedSearchGinOffsetFilter = new ExtendedSearchGinOffsetFilter
        {
            TempStoragePool = tempStoragePool,
            GeneralDirectIndex = generalDirectIndex,
            GinExtended = _ginOffsetExtended,
            RelevanceFilter = relevanceFilter
        };
    }

    /// <inheritdoc/>
    public IExtendedSearchProcessor GetSearchProcessor(ExtendedSearchType searchType)
    {
        return searchType switch
        {
            ExtendedSearchType.Legacy => _extendedSearchLegacy,
            ExtendedSearchType.GinSimple => _extendedSearchGinSimple,
            ExtendedSearchType.GinOptimized => _extendedSearchGinOptimized,
            ExtendedSearchType.GinFilter => _extendedSearchGinFilter,
            ExtendedSearchType.GinFast => _extendedSearchGinFast,
            ExtendedSearchType.GinFastFilter => _extendedSearchGinFastFilter,
            ExtendedSearchType.GinMerge => _extendedSearchGinMerge,
            ExtendedSearchType.GinMergeFilter => _extendedSearchGinMergeFilter,
            ExtendedSearchType.GinOffset => _extendedSearchGinOffset,
            ExtendedSearchType.GinOffsetFilter => _extendedSearchGinOffsetFilter,
            _ => throw new ArgumentOutOfRangeException(nameof(searchType), searchType,
                "unknown search type")
        };
    }

    /// <inheritdoc/>
    public void AddVector(DocumentId documentId, TokenVector tokenVector)
    {
        _ginExtended.AddVector(documentId, tokenVector);
        _ginOffsetExtended.AddOrUpdateVector(documentId, tokenVector);
    }

    /// <inheritdoc/>
    public void UpdateVector(DocumentId documentId, TokenVector tokenVector, TokenVector oldTokenVector)
    {
        _ginExtended.UpdateVector(documentId, tokenVector, oldTokenVector);
        _ginOffsetExtended.AddOrUpdateVector(documentId, tokenVector);
    }

    /// <inheritdoc/>
    public void RemoveVector(DocumentId documentId, TokenVector tokenVector)
    {
        _ginExtended.RemoveVector(documentId, tokenVector);
        _ginOffsetExtended.RemoveVector(documentId);
    }

    /// <inheritdoc/>
    public void Clear()
    {
        _ginExtended.Clear();
        _ginOffsetExtended.Clear();
    }
}
