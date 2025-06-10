namespace SearchEngine.Service.Tokenizer.Factory;

/// <summary>
/// Тип оптимизации поискового алгоритма.
/// </summary>
public enum SearchType
{
    /// <summary>
    /// Поиск без оптимизации, инвертированный индекс не используется.
    /// Используются алгоритмы: <see cref="ExtendedMetricsOriginal"/> и <see cref="ReducedMetricsOriginal"/>
    /// </summary>
    Original = 0,

    /// <summary>
    /// Оптимизация на инвертированном индексе.
    /// GIN используется для формирования пространства поиска, итерация по поисковому запросу.
    /// Используются алгоритмы: <see cref="ExtendedSearchGinOptimized"/> и <see cref="ReducedMetricsGinOptimized"/>
    /// </summary>
    GinOptimized = 1,

    /// <summary>
    /// Вариант оптимизации на инвертированном индексе.
    /// GIN используется для сокращения пространства поиска, итерация по индексу.
    /// Используются алгоритмы: <see cref="ExtendedMetricsGin"/> и <see cref="ReducedMetricsGin"/>
    /// </summary>
    GinSimple = 2
}
