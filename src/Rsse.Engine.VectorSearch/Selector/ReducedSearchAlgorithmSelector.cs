using System;
using RsseEngine.Algorithms;
using RsseEngine.Contracts;
using RsseEngine.Dto;
using RsseEngine.Indexes;
using RsseEngine.SearchType;

namespace RsseEngine.Selector;

/// <summary>
/// Компонент, предоставляющий доступ к различным алгоритмам reduced-поиска.
/// </summary>
public sealed class ReducedSearchAlgorithmSelector
    : ISearchAlgorithmSelector<ReducedSearchType, IReducedSearchProcessor>
{
    /// <summary>
    /// Поддержка инвертированного индекса для сокращенного поиска и метрик.
    /// </summary>
    private readonly InvertedIndex<DocumentIdSet> _ginReduced = new();

    private readonly ReducedSearchLegacy _reducedSearchLegacy;
    private readonly ReducedSearchGin _reducedSearchGin;
    private readonly ReducedSearchGinOptimized _reducedSearchGinOptimized;
    private readonly ReducedSearchGinFast _reducedSearchGinFast;

    /// <summary>
    /// Компонент с reduced-алгоритмами.
    /// </summary>
    /// <param name="generalDirectIndex">Общий индекс.</param>
    public ReducedSearchAlgorithmSelector(DirectIndex generalDirectIndex)
    {
        // Без GIN-индекса.
        _reducedSearchLegacy = new ReducedSearchLegacy
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

    /// <inheritdoc/>
    public IReducedSearchProcessor GetSearchProcessor(ReducedSearchType searchType)
    {
        return searchType switch
        {
            ReducedSearchType.Legacy => _reducedSearchLegacy,
            ReducedSearchType.GinSimple => _reducedSearchGin,
            ReducedSearchType.GinOptimized => _reducedSearchGinOptimized,
            ReducedSearchType.GinFast => _reducedSearchGinFast,
            _ => throw new ArgumentOutOfRangeException(nameof(searchType), searchType,
                "unknown search type")
        };
    }

    /// <inheritdoc/>
    public void AddVector(DocumentId documentId, TokenVector tokenVector)
    {
        _ginReduced.AddVector(documentId, tokenVector);
    }

    /// <inheritdoc/>
    public void UpdateVector(DocumentId documentId, TokenVector tokenVector, TokenVector oldTokenVector)
    {
        _ginReduced.UpdateVector(documentId, tokenVector, oldTokenVector);
    }

    /// <inheritdoc/>
    public void RemoveVector(DocumentId documentId, TokenVector tokenVector)
    {
        _ginReduced.RemoveVector(documentId, tokenVector);
    }

    /// <inheritdoc/>
    public void Clear()
    {
        _ginReduced.Clear();
    }
}
