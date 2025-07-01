using RsseEngine.Algorithms;

namespace RsseEngine.SearchType;

/// <summary>
/// Тип оптимизации reduced-алгоритмов поиска.
/// </summary>
public enum ReducedSearchType
{
    /// <summary>
    /// "Оригинальный" поиск без оптимизации, инвертированный индекс не используется.
    /// Используются алгоритмы: <see cref="ExtendedSearchLegacy"/> и <see cref="ReducedSearchLegacy"/>
    /// </summary>
    Legacy = 0,

    /// <summary>
    /// Вариант оптимизации на инвертированном индексе.
    /// GIN используется для сокращения пространства поиска.
    /// Используются алгоритмы: <see cref="ExtendedSearchGin"/> и <see cref="ReducedSearchGin"/>
    /// </summary>
    GinSimple = 1,

    /// <summary>
    /// Вариант оптимизации на инвертированном индексе.
    /// GIN используется для сокращения пространства поиска.
    /// Используются алгоритмы: <see cref="ExtendedSearchGin"/> и <see cref="ReducedSearchGin"/>
    /// </summary>
    GinSimpleFilter = 2,

    /// <summary>
    /// Оптимизация на инвертированном индексе.
    /// GIN используется для формирования пространства поиска.
    /// Используются алгоритмы: <see cref="ExtendedSearchGinOptimized"/> и <see cref="ReducedSearchGinOptimized"/>
    /// </summary>
    GinOptimized = 3,

    /// <summary>
    /// Оптимизация на инвертированном индексе.
    /// GIN используется для формирования пространства поиска.
    /// Используются алгоритмы: <see cref="ExtendedSearchGinOptimized"/> и <see cref="ReducedSearchGinOptimized"/>
    /// </summary>
    GinOptimizedFilter = 4,

    /// <summary>
    /// Вариант оптимизации на инвертированном индексе.
    /// GIN используется для сокращения пространства поиска, применены дополнительные оптимизации.
    /// Используются алгоритмы: <see cref="ExtendedSearchGinFast"/> и <see cref="ReducedSearchGinFast"/>
    /// </summary>
    GinFast = 5,

    /// <summary>
    /// Вариант оптимизации на инвертированном индексе.
    /// GIN используется для сокращения пространства поиска, применены дополнительные оптимизации.
    /// Используются алгоритмы: <see cref="ExtendedSearchGinFast"/> и <see cref="ReducedSearchGinFast"/>
    /// </summary>
    GinFastFilter = 6
}
