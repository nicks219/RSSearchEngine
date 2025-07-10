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
public sealed class ReducedSearchAlgorithmSelector<TDocumentIdCollection>
    : ISearchAlgorithmSelector<ReducedSearchType, IReducedSearchProcessor>
    where TDocumentIdCollection : struct, IDocumentIdCollection<TDocumentIdCollection>
{
    /// <summary>
    /// Поддержка инвертированного индекса для сокращенного поиска и метрик.
    /// </summary>
    private readonly InvertedIndex<TDocumentIdCollection> _ginReduced = new();

    private readonly ReducedSearchLegacy _reducedSearchLegacy;
    private readonly ReducedSearchGinSimple<TDocumentIdCollection> _reducedSearchGinSimple;
    private readonly ReducedSearchGinOptimized<TDocumentIdCollection> _reducedSearchGinOptimized;
    private readonly ReducedSearchGinOptimizedFilter<TDocumentIdCollection> _reducedSearchGinOptimizedFilter;
    private readonly ReducedSearchGinFilter<TDocumentIdCollection> _reducedSearchGinFilter;
    private readonly ReducedSearchGinFast<TDocumentIdCollection> _reducedSearchGinFast;
    private readonly ReducedSearchGinFastFilter<TDocumentIdCollection> _reducedSearchGinFastFilter;
    private readonly IReducedSearchProcessor _reducedSearchGinMerge1;
    private readonly IReducedSearchProcessor _reducedSearchGinMergeFilter1;
    private readonly IReducedSearchProcessor _reducedSearchGinMerge2;
    private readonly IReducedSearchProcessor _reducedSearchGinMergeFilter2;

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
        _reducedSearchGinSimple = new ReducedSearchGinSimple<TDocumentIdCollection>
        {
            GeneralDirectIndex = generalDirectIndex,
            GinReduced = _ginReduced
        };

        // С GIN-индексом.
        _reducedSearchGinOptimized = new ReducedSearchGinOptimized<TDocumentIdCollection>
        {
            TempStoragePool = tempStoragePool,
            GeneralDirectIndex = generalDirectIndex,
            GinReduced = _ginReduced
        };

        // С GIN-индексом.
        _reducedSearchGinOptimizedFilter = new ReducedSearchGinOptimizedFilter<TDocumentIdCollection>
        {
            TempStoragePool = tempStoragePool,
            GeneralDirectIndex = generalDirectIndex,
            GinReduced = _ginReduced,
            RelevanceFilter = relevanceFilter
        };

        _reducedSearchGinFilter = new ReducedSearchGinFilter<TDocumentIdCollection>
        {
            TempStoragePool = tempStoragePool,
            GeneralDirectIndex = generalDirectIndex,
            GinReduced = _ginReduced,
            RelevanceFilter = relevanceFilter
        };

        // С GIN-индексом.
        _reducedSearchGinFast = new ReducedSearchGinFast<TDocumentIdCollection>
        {
            TempStoragePool = tempStoragePool,
            GeneralDirectIndex = generalDirectIndex,
            GinReduced = _ginReduced
        };

        // С GIN-индексом.
        _reducedSearchGinFastFilter = new ReducedSearchGinFastFilter<TDocumentIdCollection>
        {
            TempStoragePool = tempStoragePool,
            GeneralDirectIndex = generalDirectIndex,
            GinReduced = _ginReduced,
            RelevanceFilter = relevanceFilter
        };

        if (typeof(TDocumentIdCollection) == typeof(DocumentIdList))
        {
            _reducedSearchGinMerge1 = new ReducedSearchGinMerge1
            {
                TempStoragePool = tempStoragePool,
                GeneralDirectIndex = generalDirectIndex,
                GinReduced = (InvertedIndex<DocumentIdList>)(object)_ginReduced,
                RelevanceFilter = relevanceFilter,
                EnableRelevanceFilter = false
            };

            _reducedSearchGinMergeFilter1 = new ReducedSearchGinMerge1
            {
                TempStoragePool = tempStoragePool,
                GeneralDirectIndex = generalDirectIndex,
                GinReduced = (InvertedIndex<DocumentIdList>)(object)_ginReduced,
                RelevanceFilter = relevanceFilter,
                EnableRelevanceFilter = true
            };

            _reducedSearchGinMerge2 = new ReducedSearchGinMerge2
            {
                TempStoragePool = tempStoragePool,
                GeneralDirectIndex = generalDirectIndex,
                GinReduced = (InvertedIndex<DocumentIdList>)(object)_ginReduced,
                RelevanceFilter = relevanceFilter,
                EnableRelevanceFilter = false
            };

            _reducedSearchGinMergeFilter2 = new ReducedSearchGinMerge2
            {
                TempStoragePool = tempStoragePool,
                GeneralDirectIndex = generalDirectIndex,
                GinReduced = (InvertedIndex<DocumentIdList>)(object)_ginReduced,
                RelevanceFilter = relevanceFilter,
                EnableRelevanceFilter = true
            };
        }
        else if (typeof(TDocumentIdCollection) == typeof(DocumentIdSet))
        {
            // Fallback для DocumentIdSet
            _reducedSearchGinMerge1 = _reducedSearchGinOptimizedFilter;
            _reducedSearchGinMergeFilter1 = _reducedSearchGinOptimizedFilter;
            _reducedSearchGinMerge2 = _reducedSearchGinOptimizedFilter;
            _reducedSearchGinMergeFilter2 = _reducedSearchGinOptimizedFilter;
        }
        else
        {
            throw new NotSupportedException($"[{nameof(TDocumentIdCollection)}] is not supported.");
        }
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
            ReducedSearchType.GinMerge1 => _reducedSearchGinMerge1,
            ReducedSearchType.GinMergeFilter1 => _reducedSearchGinMergeFilter1,
            ReducedSearchType.GinMerge2 => _reducedSearchGinMerge2,
            ReducedSearchType.GinMergeFilter2 => _reducedSearchGinMergeFilter2,
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
