using RsseEngine.Algorithms;

namespace RsseEngine.SearchType;

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
    /// Используется алгоритм <see cref="ReducedSearchGinOptimized"/>
    /// </summary>
    GinOptimized = 1,

    /// <summary>
    /// Оптимизация на инвертированном индексе.
    /// Используется алгоритм <see cref="ReducedSearchGinOptimizedFilter"/>
    /// </summary>
    GinOptimizedFilter = 2,

    /// <summary>
    /// Оптимизация на инвертированном индексе.
    /// Используется алгоритм <see cref="ReducedSearchGinFilter"/>
    /// </summary>
    GinFilter = 3,

    /// <summary>
    /// Оптимизация на инвертированном индексе.
    /// Используется алгоритм <see cref="ReducedSearchGinFast"/>
    /// </summary>
    GinFast = 4,

    /// <summary>
    /// Оптимизация на инвертированном индексе.
    /// Используется алгоритм <see cref="ReducedSearchGinFastFilter"/>
    /// </summary>
    GinFastFilter = 5,

    /// <summary>
    /// Оптимизация на инвертированном индексе.
    /// Используется алгоритм <see cref="ReducedSearchGinMerge"/>
    /// </summary>
    GinMerge = 6,

    /// <summary>
    /// Оптимизация на инвертированном индексе.
    /// Используется алгоритм <see cref="ReducedSearchGinMerge"/>
    /// </summary>
    GinMergeFilter = 7
}
