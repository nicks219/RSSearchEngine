using SimpleEngine.Algorithms;

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
    /// Оптимизация на инвертированном индексе.
    /// Используется алгоритм <see cref="ExtendedSearchGinOffset"/>
    /// </summary>
    Offset = 1,

    /// <summary>
    /// Оптимизация на инвертированном индексе.
    /// Используется алгоритм <see cref="ExtendedSearchGinOffsetFilter"/>
    /// </summary>
    OffsetFilter = 2,

    /// <summary>
    /// Оптимизация на инвертированном индексе, линейный поиск.
    /// Используется алгоритм <see cref="ExtendedSearchGinArrayDirect"/>
    /// </summary>
    DirectLinear = 3,

    /// <summary>
    /// Оптимизация на инвертированном индексе, линейный поиск.
    /// Используется алгоритм <see cref="ExtendedSearchGinArrayDirectFilter"/>
    /// </summary>
    DirectFilterLinear = 4,

    /// <summary>
    /// Оптимизация на инвертированном индексе, бинарный поиск.
    /// Используется алгоритм <see cref="ExtendedSearchGinArrayDirect"/>
    /// </summary>
    DirectBinary = 5,

    /// <summary>
    /// Оптимизация на инвертированном индексе, бинарный поиск.
    /// Используется алгоритм <see cref="ExtendedSearchGinArrayDirectFilter"/>
    /// </summary>
    DirectFilterBinary = 6,

    /// <summary>
    /// Оптимизация на инвертированном индексе, поиск по хэш-таблице.
    /// Используется алгоритм <see cref="ExtendedSearchGinArrayDirect"/>
    /// </summary>
    DirectHash = 7,

    /// <summary>
    /// Оптимизация на инвертированном индексе, поиск по хэш-таблице.
    /// Используется алгоритм <see cref="ExtendedSearchGinArrayDirectFilter"/>
    /// </summary>
    DirectFilterHash = 8
}
