using RD.RsseEngine.Algorithms;

namespace RD.RsseEngine.SearchType;

/// <summary>
/// Тип оптимизации reduced-алгоритмов поиска.
/// </summary>
public enum ReducedSearchType
{
    /// <summary>
    /// "Оригинальный" поиск без оптимизации, инвертированный индекс не используется.
    /// Используется алгоритм <see cref="ReducedSearchLegacy"/>
    /// </summary>
    Legacy = 0,

    /// <summary>
    /// Оптимизация на инвертированном индексе.
    /// Используется алгоритм <see cref="ReducedSearchGinArrayDirect"/>
    /// </summary>
    GinArrayDirect = 1,

    /// <summary>
    /// Оптимизация на инвертированном индексе.
    /// Используется алгоритм <see cref="ReducedSearchGinArrayMergeFilter"/>
    /// </summary>
    GinArrayMergeFilter = 2,

    /// <summary>
    /// Оптимизация на инвертированном индексе.
    /// Используется алгоритм <see cref="ReducedSearchGinArrayDirectFilter"/>
    /// </summary>
    GinArrayDirectFilterLs = 3,

    /// <summary>
    /// Оптимизация на инвертированном индексе.
    /// Используется алгоритм <see cref="ReducedSearchGinArrayDirectFilter"/>
    /// </summary>
    GinArrayDirectFilterBs = 4,

    /// <summary>
    /// Оптимизация на инвертированном индексе.
    /// Используется алгоритм <see cref="ReducedSearchGinArrayDirectFilter"/>
    /// </summary>
    GinArrayDirectFilterHs = 5
}
