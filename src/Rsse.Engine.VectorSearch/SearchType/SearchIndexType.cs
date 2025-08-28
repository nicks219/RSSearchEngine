using System;

namespace RsseEngine.SearchType;

/// <summary>
/// Тип поискового индекса.
/// </summary>
[Flags]
public enum SearchIndexType
{
    None = 0,

    Direct = 1 << 0,

    InvertedIndexExtended = 1 << 1,

    InvertedIndexReduced = 1 << 2,

    InvertedIndexHsExtended = 1 << 3,

    InvertedIndexHsReduced = 1 << 4,

    InvertedOffsetIndexExtended = 1 << 5,

    All = int.MaxValue
}
