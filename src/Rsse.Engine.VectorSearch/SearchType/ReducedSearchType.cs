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
    /// Используется алгоритм <see cref="ReducedSearchGinSimple"/>
    /// </summary>
    GinSimple = 1,

    /// <summary>
    /// Оптимизация на инвертированном индексе.
    /// Используется алгоритм <see cref="ReducedSearchGinOptimized"/>
    /// </summary>
    GinOptimized = 2,

    /// <summary>
    /// Оптимизация на инвертированном индексе.
    /// Используется алгоритм <see cref="ReducedSearchGinOptimizedFilter"/>
    /// </summary>
    GinOptimizedFilter = 3,

    /// <summary>
    /// Оптимизация на инвертированном индексе.
    /// Используется алгоритм <see cref="ReducedSearchGinFilter"/>
    /// </summary>
    GinFilter = 4,

    /// <summary>
    /// Оптимизация на инвертированном индексе.
    /// Используется алгоритм <see cref="ReducedSearchGinFast"/>
    /// </summary>
    GinFast = 5,

    /// <summary>
    /// Оптимизация на инвертированном индексе.
    /// Используется алгоритм <see cref="ReducedSearchGinFastFilter"/>
    /// </summary>
    GinFastFilter = 6,

    /// <summary>
    /// Оптимизация на инвертированном индексе.
    /// Используется алгоритм <see cref="ReducedSearchGinMerge1"/>
    /// </summary>
    GinMerge1 = 7,

    /// <summary>
    /// Оптимизация на инвертированном индексе.
    /// Используется алгоритм <see cref="ReducedSearchGinMerge1"/>
    /// </summary>
    GinMergeFilter1 = 8,

    /// <summary>
    /// Оптимизация на инвертированном индексе.
    /// Используется алгоритм <see cref="ReducedSearchGinMerge2"/>
    /// </summary>
    GinMerge2 = 9,

    /// <summary>
    /// Оптимизация на инвертированном индексе.
    /// Используется алгоритм <see cref="ReducedSearchGinMerge2"/>
    /// </summary>
    GinMergeFilter2 = 10
}
