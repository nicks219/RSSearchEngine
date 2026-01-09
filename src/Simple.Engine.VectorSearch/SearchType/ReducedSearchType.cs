using SimpleEngine.Algorithms;
using SimpleEngine.Algorithms.Legacy;

namespace SimpleEngine.SearchType;

/// <summary>
/// Варианты конфигураций для различных reduced-алгоритмов поиска.
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
    Direct = 1,

    SimpleLegacy = 2,

    /// <summary>
    /// Оптимизация на инвертированном индексе.
    /// Используется алгоритм <see cref="ReducedSearchGinArrayMergeFilter"/>
    /// </summary>
    //MergeFilter = 2,

    /// <summary>
    /// Оптимизация на инвертированном индексе, линейный поиск.
    /// Используется алгоритм <see cref="ReducedSearchGinArrayDirectFilter"/>
    /// </summary>
    //DirectFilterLinear = 3,

    /// <summary>
    /// Оптимизация на инвертированном индексе, бинарный поиск.
    /// Используется алгоритм <see cref="ReducedSearchGinArrayDirectFilter"/>
    /// </summary>
    //DirectFilterBinary = 4,

    /// <summary>
    /// Оптимизация на инвертированном индексе, поиск по хэш-таблице.
    /// Используется алгоритм <see cref="ReducedSearchGinArrayDirectFilter"/>
    /// </summary>
    //DirectFilterHash = 5
}
