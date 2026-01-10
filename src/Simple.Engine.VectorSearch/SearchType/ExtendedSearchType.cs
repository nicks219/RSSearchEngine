using SimpleEngine.Algorithms;
using SimpleEngine.Algorithms.Legacy;

namespace SimpleEngine.SearchType;

/// <summary>
/// Варианты конфигураций для различных extended-алгоритмов поиска.
/// </summary>
public enum ExtendedSearchType
{
    /// <summary>
    /// "Оригинальный" поиск без оптимизации, инвертированный индекс не используется.
    /// Используется алгоритм <see cref="ExtendedSearchLegacy"/>
    /// </summary>
    Legacy = 0,

    /// <summary>
    /// Неоптимизированный алгоритм на инвертированном индексе.
    /// Используется алгоритм <see cref="ExtendedSearchSimple"/>
    /// </summary>
    SimpleLegacy = 2,

    /// <summary>
    /// Оптимизированный алгоритм на инвертированном индексе, линейный поиск.
    /// Используется алгоритм <see cref="ExtendedSearchGinArrayDirect"/>
    /// </summary>
    DirectLinear = 3,

    /// <summary>
    /// Оптимизированный алгоритм на инвертированном индексе, бинарный поиск.
    /// Используется алгоритм <see cref="ExtendedSearchGinArrayDirect"/>
    /// </summary>
    DirectBinary = 5,

    /// <summary>
    /// Оптимизированный алгоритм на инвертированном индексе, поиск по хэш-таблице.
    /// Используется алгоритм <see cref="ExtendedSearchGinArrayDirect"/>
    /// </summary>
    DirectHash = 7,
}
