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
    /// Оптимизированный алгоритм на инвертированном индексе.
    /// Используется алгоритм <see cref="ReducedSearchGinArrayDirect"/>
    /// </summary>
    Direct = 1,

    /// <summary>
    /// Неоптимизированный алгоритм на инвертированном индексе.
    /// Используется алгоритм <see cref="ReducedSearchSimple"/>
    /// </summary>
    SimpleLegacy = 2,
}
