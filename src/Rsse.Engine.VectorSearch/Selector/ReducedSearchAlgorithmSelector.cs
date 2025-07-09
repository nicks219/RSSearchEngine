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
    private readonly ReducedSearchGinSimple<DocumentIdSet> _reducedSearchGinSimple;
    private readonly ReducedSearchGinOptimized<DocumentIdSet> _reducedSearchGinOptimized;
    private readonly ReducedSearchGinOptimizedFilter<DocumentIdSet> _reducedSearchGinOptimizedFilter;
    private readonly ReducedSearchGinFilter<DocumentIdSet> _reducedSearchGinFilter;
    private readonly ReducedSearchGinFast<DocumentIdSet> _reducedSearchGinFast;
    private readonly ReducedSearchGinFastFilter<DocumentIdSet> _reducedSearchGinFastFilter;

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
            TempStoragePool = tempStoragePool,
            GeneralDirectIndex = generalDirectIndex
        };

        // С GIN-индексом.
        _reducedSearchGinSimple = new ReducedSearchGinSimple<DocumentIdSet>
        {
            GeneralDirectIndex = generalDirectIndex,
            GinReduced = _ginReduced
        };

        // С GIN-индексом.
        _reducedSearchGinOptimized = new ReducedSearchGinOptimized<DocumentIdSet>
        {
            TempStoragePool = tempStoragePool,
            GeneralDirectIndex = generalDirectIndex,
            GinReduced = _ginReduced
        };

        // С GIN-индексом.
        _reducedSearchGinOptimizedFilter = new ReducedSearchGinOptimizedFilter<DocumentIdSet>
        {
            TempStoragePool = tempStoragePool,
            GeneralDirectIndex = generalDirectIndex,
            GinReduced = _ginReduced,
            RelevanceFilter = relevanceFilter
        };

        _reducedSearchGinFilter = new ReducedSearchGinFilter<DocumentIdSet>
        {
            TempStoragePool = tempStoragePool,
            GeneralDirectIndex = generalDirectIndex,
            GinReduced = _ginReduced,
            RelevanceFilter = relevanceFilter
        };

        // С GIN-индексом.
        _reducedSearchGinFast = new ReducedSearchGinFast<DocumentIdSet>
        {
            TempStoragePool = tempStoragePool,
            GeneralDirectIndex = generalDirectIndex,
            GinReduced = _ginReduced
        };

        // С GIN-индексом.
        _reducedSearchGinFastFilter = new ReducedSearchGinFastFilter<DocumentIdSet>
        {
            TempStoragePool = tempStoragePool,
            GeneralDirectIndex = generalDirectIndex,
            GinReduced = _ginReduced,
            RelevanceFilter = relevanceFilter
        };
    }

    /// <inheritdoc/>
    public IReducedSearchProcessor GetSearchProcessor(ReducedSearchType searchType)
    {
        return searchType switch
        {
            ReducedSearchType.Legacy => _reducedSearchLegacy,
            ReducedSearchType.GinSimple => _reducedSearchGinSimple,
            ReducedSearchType.GinOptimized => _reducedSearchGinOptimized,
            ReducedSearchType.GinOptimizedFilter => _reducedSearchGinOptimizedFilter,
            ReducedSearchType.GinFilter => _reducedSearchGinFilter,
            ReducedSearchType.GinFast => _reducedSearchGinFast,
            ReducedSearchType.GinFastFilter => _reducedSearchGinFastFilter,
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
