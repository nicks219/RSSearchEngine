using System;
using RsseEngine.Algorithms;
using RsseEngine.Contracts;
using RsseEngine.Dto;
using RsseEngine.Indexes;
using RsseEngine.SearchType;

namespace RsseEngine.Selector;

public sealed class ExtendedSearchAlgorithmSelector
    : ISearchAlgorithmSelector<ExtendedSearchType, IExtendedSearchProcessor>
{
    /// <summary>
    /// Поддержка GIN-индекса для расширенного поиска и метрик.
    /// </summary>
    private readonly InverseIndex<DocumentIdSet> _ginExtended = new();

    private readonly ExtendedSearchLegacy _extendedSearchLegacy;
    private readonly ExtendedSearchGin _extendedSearchGin;
    private readonly ExtendedSearchGinOptimized _extendedSearchGinOptimized;
    private readonly ExtendedSearchGinFast _extendedSearchGinFast;

    public ExtendedSearchAlgorithmSelector(DirectIndex generalDirectIndex)
    {
        // Без GIN-индекса.
        _extendedSearchLegacy = new ExtendedSearchLegacy
        {
            GeneralDirectIndex = generalDirectIndex
        };

        // С GIN-индексом.
        _extendedSearchGin = new ExtendedSearchGin
        {
            GeneralDirectIndex = generalDirectIndex,
            GinExtended = _ginExtended
        };

        // С GIN-индексом.
        _extendedSearchGinOptimized = new ExtendedSearchGinOptimized
        {
            GeneralDirectIndex = generalDirectIndex,
            GinExtended = _ginExtended
        };

        // С GIN-индексом.
        _extendedSearchGinFast = new ExtendedSearchGinFast
        {
            GeneralDirectIndex = generalDirectIndex,
            GinExtended = _ginExtended
        };
    }

    public IExtendedSearchProcessor GetSearchProcessor(ExtendedSearchType searchType)
    {
        return searchType switch
        {
            ExtendedSearchType.Legacy => _extendedSearchLegacy,
            ExtendedSearchType.GinSimple => _extendedSearchGin,
            ExtendedSearchType.GinOptimized => _extendedSearchGinOptimized,
            ExtendedSearchType.GinFast => _extendedSearchGinFast,
            _ => throw new ArgumentOutOfRangeException(nameof(searchType), searchType,
                "unknown search type")
        };
    }

    public void AddVector(DocumentId documentId, TokenVector tokenVector)
    {
        _ginExtended.AddVector(documentId, tokenVector);
    }

    public void UpdateVector(DocumentId documentId, TokenVector tokenVector, TokenVector oldTokenVector)
    {
        _ginExtended.UpdateVector(documentId, tokenVector, oldTokenVector);
    }

    public void RemoveVector(DocumentId documentId, TokenVector tokenVector)
    {
        _ginExtended.RemoveVector(documentId, tokenVector);
    }

    public void Clear()
    {
        _ginExtended.Clear();
    }
}
