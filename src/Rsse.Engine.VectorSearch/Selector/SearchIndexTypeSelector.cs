using System;
using RsseEngine.SearchType;

namespace RsseEngine.Selector;

public static class SearchIndexTypeSelector
{
    public static SearchIndexType GetIndexType(ExtendedSearchType extendedSearchType, ReducedSearchType reducedSearchType)
    {
        return GetIndexType(extendedSearchType) | GetIndexType(reducedSearchType);
    }

    public static SearchIndexType GetIndexType(ExtendedSearchType searchType)
    {
        return searchType switch
        {
            ExtendedSearchType.Legacy => SearchIndexType.Direct,
            ExtendedSearchType.GinOffset => SearchIndexType.InvertedOffsetIndexExtended,
            ExtendedSearchType.GinOffsetFilter => SearchIndexType.InvertedOffsetIndexExtended,
            ExtendedSearchType.GinArrayDirectLs => SearchIndexType.InvertedIndexExtended,
            ExtendedSearchType.GinArrayDirectFilterLs => SearchIndexType.InvertedIndexExtended,
            ExtendedSearchType.GinArrayDirectBs => SearchIndexType.InvertedIndexExtended,
            ExtendedSearchType.GinArrayDirectFilterBs => SearchIndexType.InvertedIndexExtended,
            ExtendedSearchType.GinArrayDirectHs => SearchIndexType.InvertedIndexHsExtended,
            ExtendedSearchType.GinArrayDirectFilterHs => SearchIndexType.InvertedIndexHsExtended,
            _ => throw new ArgumentOutOfRangeException(nameof(searchType), searchType, "unknown search type")
        };
    }

    public static SearchIndexType GetIndexType(ReducedSearchType searchType)
    {
        return searchType switch
        {
            ReducedSearchType.Legacy => SearchIndexType.Direct,
            ReducedSearchType.GinArrayDirect => SearchIndexType.InvertedIndexReduced,
            ReducedSearchType.GinArrayMergeFilter => SearchIndexType.InvertedIndexReduced,
            ReducedSearchType.GinArrayDirectFilterLs => SearchIndexType.InvertedIndexReduced,
            ReducedSearchType.GinArrayDirectFilterBs => SearchIndexType.InvertedIndexReduced,
            ReducedSearchType.GinArrayDirectFilterHs => SearchIndexType.InvertedIndexHsReduced,
            _ => throw new ArgumentOutOfRangeException(nameof(searchType), searchType, "unknown search type")
        };
    }
}
