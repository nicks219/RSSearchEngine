using System;
using RsseEngine.Algorithms;
using RsseEngine.Contracts;
using RsseEngine.Dto;
using RsseEngine.Indexes;
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
    private readonly InvertedIndex<DocumentIdSet> _ginExtended = new();

    private readonly ExtendedSearchLegacy _extendedSearchLegacy;
    private readonly ExtendedSearchGin _extendedSearchGin;
    private readonly ExtendedSearchGin _extendedSearchGinFilter;
    private readonly ExtendedSearchGinOptimized _extendedSearchGinOptimized;
    private readonly ExtendedSearchGinOptimized _extendedSearchGinOptimizedFilter;
    private readonly ExtendedSearchGinFast _extendedSearchGinFast;
    private readonly ExtendedSearchGinFast _extendedSearchGinFastFilter;

    /// <summary>
    /// Компонент с extended-алгоритмами.
    /// </summary>
    /// <param name="generalDirectIndex">Общий индекс.</param>
    /// <param name="relevancyThreshold">Порог релевантности</param>
    public ExtendedSearchAlgorithmSelector(DirectIndex generalDirectIndex, double relevancyThreshold)
    {
        GinRelevanceFilter disabledFilter = new GinRelevanceFilter
        {
            Enabled = false,
            Threshold = relevancyThreshold
        };

        GinRelevanceFilter enabledFilter = new GinRelevanceFilter
        {
            Enabled = true,
            Threshold = relevancyThreshold
        };

        // Без GIN-индекса.
        _extendedSearchLegacy = new ExtendedSearchLegacy
        {
            GeneralDirectIndex = generalDirectIndex
        };

        // С GIN-индексом.
        _extendedSearchGin = new ExtendedSearchGin
        {
            GeneralDirectIndex = generalDirectIndex,
            GinExtended = _ginExtended,
            RelevanceFilter = disabledFilter
        };

        _extendedSearchGinFilter = new ExtendedSearchGin
        {
            GeneralDirectIndex = generalDirectIndex,
            GinExtended = _ginExtended,
            RelevanceFilter = enabledFilter
        };

        // С GIN-индексом.
        _extendedSearchGinOptimized = new ExtendedSearchGinOptimized
        {
            GeneralDirectIndex = generalDirectIndex,
            GinExtended = _ginExtended,
            RelevanceFilter = disabledFilter
        };

        _extendedSearchGinOptimizedFilter = new ExtendedSearchGinOptimized
        {
            GeneralDirectIndex = generalDirectIndex,
            GinExtended = _ginExtended,
            RelevanceFilter = enabledFilter
        };

        // С GIN-индексом.
        _extendedSearchGinFast = new ExtendedSearchGinFast
        {
            GeneralDirectIndex = generalDirectIndex,
            GinExtended = _ginExtended,
            RelevanceFilter = disabledFilter
        };

        _extendedSearchGinFastFilter = new ExtendedSearchGinFast
        {
            GeneralDirectIndex = generalDirectIndex,
            GinExtended = _ginExtended,
            RelevanceFilter = enabledFilter
        };
    }

    /// <inheritdoc/>
    public IExtendedSearchProcessor GetSearchProcessor(ExtendedSearchType searchType)
    {
        return searchType switch
        {
            ExtendedSearchType.Legacy => _extendedSearchLegacy,
            ExtendedSearchType.GinSimple => _extendedSearchGin,
            ExtendedSearchType.GinOptimized => _extendedSearchGinOptimized,
            ExtendedSearchType.GinFast => _extendedSearchGinFast,
            ExtendedSearchType.GinSimpleFilter => _extendedSearchGinFilter,
            ExtendedSearchType.GinOptimizedFilter => _extendedSearchGinOptimizedFilter,
            ExtendedSearchType.GinFastFilter => _extendedSearchGinFastFilter,
            _ => throw new ArgumentOutOfRangeException(nameof(searchType), searchType,
                "unknown search type")
        };
    }

    /// <inheritdoc/>
    public void AddVector(DocumentId documentId, TokenVector tokenVector)
    {
        _ginExtended.AddVector(documentId, tokenVector);
    }

    /// <inheritdoc/>
    public void UpdateVector(DocumentId documentId, TokenVector tokenVector, TokenVector oldTokenVector)
    {
        _ginExtended.UpdateVector(documentId, tokenVector, oldTokenVector);
    }

    /// <inheritdoc/>
    public void RemoveVector(DocumentId documentId, TokenVector tokenVector)
    {
        _ginExtended.RemoveVector(documentId, tokenVector);
    }

    /// <inheritdoc/>
    public void Clear()
    {
        _ginExtended.Clear();
    }
}
