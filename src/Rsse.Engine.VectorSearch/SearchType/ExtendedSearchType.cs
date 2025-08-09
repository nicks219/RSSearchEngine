using RsseEngine.Algorithms;

namespace RsseEngine.SearchType;

/// <summary>
/// Тип оптимизации extended-алгоритмов поиска.
/// </summary>
public enum ExtendedSearchType
{
    /// <summary>
    /// "Оригинальный" поиск без оптимизации, инвертированный индекс не используется.
    /// Используется алгоритм <see cref="ExtendedSearchLegacy"/>
    /// </summary>
    Legacy = 0,

    /// <summary>
    /// Оптимизация на инвертированном индексе.
    /// Используется алгоритм <see cref="ExtendedSearchGinFilter"/>
    /// </summary>
    GinFilter = 1,

    /// <summary>
    /// Оптимизация на инвертированном индексе.
    /// Используется алгоритм <see cref="ExtendedSearchGinMerge"/>
    /// </summary>
    GinMerge = 2,

    /// <summary>
    /// Оптимизация на инвертированном индексе.
    /// Используется алгоритм <see cref="ExtendedSearchGinMergeFilter"/>
    /// </summary>
    GinMergeFilter = 3,

    /// <summary>
    /// Оптимизация на инвертированном индексе.
    /// Используется алгоритм <see cref="ExtendedSearchGinOffset"/>
    /// </summary>
    GinOffset = 4,

    /// <summary>
    /// Оптимизация на инвертированном индексе.
    /// Используется алгоритм <see cref="ExtendedSearchGinOffsetFilter"/>
    /// </summary>
    GinOffsetFilter = 5,

    /// <summary>
    /// Оптимизация на инвертированном индексе.
    /// Используется алгоритм <see cref="ExtendedSearchGinDirectOffset"/>
    /// </summary>
    GinDirectOffsetLs = 6,

    /// <summary>
    /// Оптимизация на инвертированном индексе.
    /// Используется алгоритм <see cref="ExtendedSearchGinDirectOffsetFilter"/>
    /// </summary>
    GinDirectOffsetFilterLs = 7,

    /// <summary>
    /// Оптимизация на инвертированном индексе.
    /// Используется алгоритм <see cref="ExtendedSearchGinDirectOffset"/>
    /// </summary>
    GinDirectOffsetBs = 8,

    /// <summary>
    /// Оптимизация на инвертированном индексе.
    /// Используется алгоритм <see cref="ExtendedSearchGinDirectOffsetFilter"/>
    /// </summary>
    GinDirectOffsetFilterBs = 9,

    /// <summary>
    /// Оптимизация на инвертированном индексе.
    /// Используется алгоритм <see cref="ExtendedSearchGinFrozenDirect"/>
    /// </summary>
    GinFrozenDirect = 10,

    /// <summary>
    /// Оптимизация на инвертированном индексе.
    /// Используется алгоритм <see cref="ExtendedSearchGinFrozenDirectFilter"/>
    /// </summary>
    GinFrozenDirectFilter = 11,

    /// <summary>
    /// Оптимизация на инвертированном индексе.
    /// Используется алгоритм <see cref="ExtendedSearchGinArrayDirect"/>
    /// </summary>
    GinArrayDirectLs = 12,

    /// <summary>
    /// Оптимизация на инвертированном индексе.
    /// Используется алгоритм <see cref="ExtendedSearchGinArrayDirectFilter"/>
    /// </summary>
    GinArrayDirectFilterLs = 13,

    /// <summary>
    /// Оптимизация на инвертированном индексе.
    /// Используется алгоритм <see cref="ExtendedSearchGinArrayDirect"/>
    /// </summary>
    GinArrayDirectBs = 14,

    /// <summary>
    /// Оптимизация на инвертированном индексе.
    /// Используется алгоритм <see cref="ExtendedSearchGinArrayDirectFilter"/>
    /// </summary>
    GinArrayDirectFilterBs = 15
}
