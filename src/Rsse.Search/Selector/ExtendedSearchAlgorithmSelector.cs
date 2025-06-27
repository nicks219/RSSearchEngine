using System;
using Rsse.Search.Algorithms;
using Rsse.Search.Dto;
using Rsse.Search.Indexes;

namespace Rsse.Search.Selector;

public sealed class ExtendedSearchAlgorithmSelector
    : ISearchAlgorithmSelector<ExtendedSearchType, IExtendedSearchProcessor>
{
    /// <summary>
    /// Поддержка GIN-индекса для расширенного поиска и метрик.
    /// </summary>
    private readonly InverseIndex<DocumentIdSet> _ginExtended = new();

    private readonly ExtendedSearch _extendedSearch;
    private readonly ExtendedSearchGin _extendedSearchGin;
    private readonly ExtendedSearchGinOptimized _extendedSearchGinOptimized;
    private readonly ExtendedSearchGinFast _extendedSearchGinFast;

    public ExtendedSearchAlgorithmSelector(DirectIndex generalDirectIndex)
    {
        // Без GIN-индекса.
        _extendedSearch = new ExtendedSearch
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
            ExtendedSearchType.Original => _extendedSearch,
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
