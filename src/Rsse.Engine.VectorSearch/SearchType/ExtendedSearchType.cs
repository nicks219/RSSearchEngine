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
    /// Используется алгоритм <see cref="ExtendedSearchGinFast"/>
    /// </summary>
    GinFast = 2,

    /// <summary>
    /// Оптимизация на инвертированном индексе.
    /// Используется алгоритм <see cref="ExtendedSearchGinFastFilter"/>
    /// </summary>
    GinFastFilter = 3,

    /// <summary>
    /// Оптимизация на инвертированном индексе.
    /// Используется алгоритм <see cref="ExtendedSearchGinMerge"/>
    /// </summary>
    GinMerge = 4,

    /// <summary>
    /// Оптимизация на инвертированном индексе.
    /// Используется алгоритм <see cref="ExtendedSearchGinMergeFilter"/>
    /// </summary>
    GinMergeFilter = 5,

    /// <summary>
    /// Оптимизация на инвертированном индексе.
    /// Используется алгоритм <see cref="ExtendedSearchGinOffset"/>
    /// </summary>
    GinOffset = 6,

    /// <summary>
    /// Оптимизация на инвертированном индексе.
    /// Используется алгоритм <see cref="ExtendedSearchGinOffsetFilter"/>
    /// </summary>
    GinOffsetFilter = 7,

    /// <summary>
    /// Оптимизация на инвертированном индексе.
    /// Используется алгоритм <see cref="ExtendedSearchGinDirectOffset"/>
    /// </summary>
    GinDirectOffset = 8,

    /// <summary>
    /// Оптимизация на инвертированном индексе.
    /// Используется алгоритм <see cref="ExtendedSearchGinDirectOffsetFilter"/>
    /// </summary>
    GinDirectOffsetFilter = 9,

    /// <summary>
    /// Оптимизация на инвертированном индексе.
    /// Используется алгоритм <see cref="ExtendedSearchGinFrozenDirect"/>
    /// </summary>
    GinFrozenDirect = 10,

    /// <summary>
    /// Оптимизация на инвертированном индексе.
    /// Используется алгоритм <see cref="ExtendedSearchGinFrozenDirectFilter"/>
    /// </summary>
    GinFrozenDirectFilter = 11
}
