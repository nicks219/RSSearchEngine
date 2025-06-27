using System;
using Rsse.Search;
using Rsse.Search.Algorithms;
using Rsse.Search.Dto;
using Rsse.Search.Indexes;

namespace SearchEngine.Service.Tokenizer.SearchProcessor;

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

    public sealed class Extended(DirectIndex generalDirectIndex) :
        ProductionSearchAlgorithmSelector,
        ISearchAlgorithmSelector<ExtendedSearchType, IExtendedSearchProcessor>
    {
        // Без GIN-индекса.
        private readonly ExtendedSearch _extendedSearch = new()
        {
            GeneralDirectIndex = generalDirectIndex
        };

        public IExtendedSearchProcessor GetSearchProcessor(ExtendedSearchType searchType)
        {
            return searchType switch
            {
                ExtendedSearchType.Original => _extendedSearch,
                _ => throw new NotSupportedException(
                    $"Extended[{searchType.ToString()}] GIN optimization is not supported in production yet.")
            };
        }
    }

    public sealed class Reduced(DirectIndex generalDirectIndex) :
        ProductionSearchAlgorithmSelector,
        ISearchAlgorithmSelector<ReducedSearchType, IReducedSearchProcessor>
    {
        // Без GIN-индекса.
        private readonly ReducedSearch _reducedSearch = new()
        {
            GeneralDirectIndex = generalDirectIndex
        };

        public IReducedSearchProcessor GetSearchProcessor(ReducedSearchType searchType)
        {
            return searchType switch
            {
                ReducedSearchType.Original => _reducedSearch,
                _ => throw new NotSupportedException(
                    $"Reduced[{searchType.ToString()}] GIN optimization is not supported in production yet.")
            };
        }
    }
}
