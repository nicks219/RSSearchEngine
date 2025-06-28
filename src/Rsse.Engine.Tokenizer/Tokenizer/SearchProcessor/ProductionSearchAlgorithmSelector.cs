using System;
using RsseEngine.Algorithms;
using RsseEngine.Contracts;
using RsseEngine.Dto;
using RsseEngine.Indexes;
using RsseEngine.SearchType;

namespace RsseEngine.Tokenizer.SearchProcessor;

public abstract class ProductionSearchAlgorithmSelector
{
    private ProductionSearchAlgorithmSelector()
    {
        // Do nothing
    }

    public void AddVector(DocumentId documentId, TokenVector tokenVector)
    {
        // Do nothing
    }

    public void UpdateVector(DocumentId documentId, TokenVector tokenVector, TokenVector oldTokenVector)
    {
        // Do nothing
    }

    public void RemoveVector(DocumentId documentId, TokenVector tokenVector)
    {
        // Do nothing
    }

    public void Clear()
    {
        // Do nothing
    }

    public sealed class ExtendedLegacy(DirectIndex generalDirectIndex) :
        ProductionSearchAlgorithmSelector,
        ISearchAlgorithmSelector<ExtendedSearchType, IExtendedSearchProcessor>
    {
        // Без GIN-индекса.
        private readonly ExtendedSearchLegacy _extendedSearchLegacy = new()
        {
            GeneralDirectIndex = generalDirectIndex
        };

        public IExtendedSearchProcessor GetSearchProcessor(ExtendedSearchType searchType)
        {
            return searchType switch
            {
                ExtendedSearchType.Legacy => _extendedSearchLegacy,
                _ => throw new NotSupportedException(
                    $"Extended[{searchType.ToString()}] GIN optimization is not supported in production yet.")
            };
        }
    }

    public sealed class ReducedLegacy(DirectIndex generalDirectIndex) :
        ProductionSearchAlgorithmSelector,
        ISearchAlgorithmSelector<ReducedSearchType, IReducedSearchProcessor>
    {
        // Без GIN-индекса.
        private readonly ReducedSearchLegacy _reducedSearchLegacy = new()
        {
            GeneralDirectIndex = generalDirectIndex
        };

        public IReducedSearchProcessor GetSearchProcessor(ReducedSearchType searchType)
        {
            return searchType switch
            {
                ReducedSearchType.Legacy => _reducedSearchLegacy,
                _ => throw new NotSupportedException(
                    $"Reduced[{searchType.ToString()}] GIN optimization is not supported in production yet.")
            };
        }
    }
}
