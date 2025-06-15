namespace SearchEngine.Service.Tokenizer.SearchProcessor;

/// <summary>
/// Тип оптимизации поискового алгоритма.
/// </summary>
public enum SearchType
{
    /// <summary>
    /// Поиск без оптимизации, инвертированный индекс не используется.
    /// Используются алгоритмы: <see cref="ExtendedSearch"/> и <see cref="ReducedSearch"/>
    /// </summary>
    Original = 0,

    /// <summary>
    /// Оптимизация на инвертированном индексе.
    /// GIN используется для формирования пространства поиска.
    /// Используются алгоритмы: <see cref="ExtendedSearchGinOptimized"/> и <see cref="ReducedSearchGinOptimized"/>
    /// </summary>
    GinOptimized = 1,

    /// <summary>
    /// Вариант оптимизации на инвертированном индексе.
    /// GIN используется для сокращения пространства поиска.
    /// Используются алгоритмы: <see cref="ExtendedSearchGin"/> и <see cref="ReducedSearchGin"/>
    /// </summary>
    GinSimple = 2,

    /// <summary>
    /// Вариант оптимизации на инвертированном индексе.
    /// GIN используется для сокращения пространства поиска, применены дополнительные оптимизации.
    /// Используются алгоритмы: <see cref="ExtendedSearchGinFast"/> и <see cref="ReducedSearchGinFast"/>
    /// </summary>
    GinFast = 3
}
