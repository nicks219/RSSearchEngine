using System;
using Rsse.Search.Algorithms;
using Rsse.Search.Dto;
using Rsse.Search.Indexes;

namespace Rsse.Search.Selector;

public sealed class ReducedSearchAlgorithmSelector
    : ISearchAlgorithmSelector<ReducedSearchType, IReducedSearchProcessor>
{
    /// <summary>
    /// Поддержка GIN-индекса для сокращенного поиска и метрик.
    /// </summary>
    private readonly InverseIndex<DocumentIdSet> _ginReduced = new();

    private readonly ReducedSearch _reducedSearch;
    private readonly ReducedSearchGin _reducedSearchGin;
    private readonly ReducedSearchGinOptimized _reducedSearchGinOptimized;
    private readonly ReducedSearchGinFast _reducedSearchGinFast;

    public ReducedSearchAlgorithmSelector(DirectIndex generalDirectIndex)
    {
        // Без GIN-индекса.
        _reducedSearch = new ReducedSearch
        {
            GeneralDirectIndex = generalDirectIndex
        };

        // С GIN-индексом.
        _reducedSearchGin = new ReducedSearchGin
        {
            GeneralDirectIndex = generalDirectIndex,
            GinReduced = _ginReduced
        };

        // С GIN-индексом.
        _reducedSearchGinOptimized = new ReducedSearchGinOptimized
        {
            GeneralDirectIndex = generalDirectIndex,
            GinReduced = _ginReduced
        };

        // С GIN-индексом.
        _reducedSearchGinFast = new ReducedSearchGinFast
        {
            GeneralDirectIndex = generalDirectIndex,
            GinReduced = _ginReduced
        };
    }

    public IReducedSearchProcessor GetSearchProcessor(ReducedSearchType searchType)
    {
        return searchType switch
        {
            ReducedSearchType.Original => _reducedSearch,
            ReducedSearchType.GinSimple => _reducedSearchGin,
            ReducedSearchType.GinOptimized => _reducedSearchGinOptimized,
            ReducedSearchType.GinFast => _reducedSearchGinFast,
            _ => throw new ArgumentOutOfRangeException(nameof(searchType), searchType,
                "unknown search type")
        };
    }

    public void AddVector(DocumentId documentId, TokenVector tokenVector)
    {
        _ginReduced.AddVector(documentId, tokenVector);
    }

    public void UpdateVector(DocumentId documentId, TokenVector tokenVector, TokenVector oldTokenVector)
    {
        _ginReduced.UpdateVector(documentId, tokenVector, oldTokenVector);
    }

    public void RemoveVector(DocumentId documentId, TokenVector tokenVector)
    {
        _ginReduced.RemoveVector(documentId, tokenVector);
    }

    public void Clear()
    {
        _ginReduced.Clear();
    }
}
