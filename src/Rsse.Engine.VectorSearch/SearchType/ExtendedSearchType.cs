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
    /// Используется алгоритм <see cref="ExtendedSearchGinOffset"/>
    /// </summary>
    GinOffset = 1,

    /// <summary>
    /// Оптимизация на инвертированном индексе.
    /// Используется алгоритм <see cref="ExtendedSearchGinOffsetFilter"/>
    /// </summary>
    GinOffsetFilter = 2,

    /// <summary>
    /// Оптимизация на инвертированном индексе.
    /// Используется алгоритм <see cref="ExtendedSearchGinArrayDirect"/>
    /// </summary>
    GinArrayDirectLs = 3,

    /// <summary>
    /// Оптимизация на инвертированном индексе.
    /// Используется алгоритм <see cref="ExtendedSearchGinArrayDirectFilter"/>
    /// </summary>
    GinArrayDirectFilterLs = 4,

    /// <summary>
    /// Оптимизация на инвертированном индексе.
    /// Используется алгоритм <see cref="ExtendedSearchGinArrayDirect"/>
    /// </summary>
    GinArrayDirectBs = 5,

    /// <summary>
    /// Оптимизация на инвертированном индексе.
    /// Используется алгоритм <see cref="ExtendedSearchGinArrayDirectFilter"/>
    /// </summary>
    GinArrayDirectFilterBs = 6,

    /// <summary>
    /// Оптимизация на инвертированном индексе.
    /// Используется алгоритм <see cref="ExtendedSearchGinArrayDirect"/>
    /// </summary>
    GinArrayDirectHs = 7,

    /// <summary>
    /// Оптимизация на инвертированном индексе.
    /// Используется алгоритм <see cref="ExtendedSearchGinArrayDirectFilter"/>
    /// </summary>
    GinArrayDirectFilterHs = 8
}
