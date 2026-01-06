using RsseEngine.SearchType;

namespace RsseEngine.Benchmarks;

/// <summary>
/// Общие константы для бенчмарков.
/// </summary>
public abstract class Constants
{
    /// <summary>
    /// Количество запусков в рамках профилирования.
    /// </summary>
    internal const int ProfilerIterations = 1000;

    /// <summary>
    /// Количество запусков в рамках прогрева.
    /// </summary>
    internal const int WarmUpIterations = 10;

    /// <summary>
    /// Измеряемый алгоритм токенизатора.
    /// </summary>
    internal const ExtendedSearchType TokenizerExtendedSearchType = ExtendedSearchType.DirectFilterLinear;

    /// <summary>
    /// Измеряемый алгоритм токенизатора.
    /// </summary>
    internal const ReducedSearchType TokenizerReducedSearchType = ReducedSearchType.DirectFilterLinear;

    /// <summary>
    /// Константа с поисковым запросом.
    /// </summary>
    public const string SearchQuery = "приключится вдруг вот верный друг выручить";

    // public const string SearchQuery = "преключиться вдруг верный друг";
    // public const string SearchQuery = "пляшем на столе за детей";
    // public const string SearchQuery = "приключится вдруг верный друг";
}
