namespace SearchEngine.Benchmarks.Common;

/// <summary>
/// Общие константы для бенчмарков.
/// </summary>
public abstract class Constants
{
    /// <summary>
    /// Сколько раз раскопировать тестовые данные.
    /// </summary>
    internal const int InitialDataMultiplier = 50;

    /// <summary>
    /// Количество запусков в рамках профилирования.
    /// </summary>
    internal const int ProfilerIterations = 5;

    /// <summary>
    /// Количество запусков в рамках прогрева.
    /// </summary>
    internal const int WarmUpIterations = 1;

    /// <summary>
    /// Константа с поисковым запросом.
    /// </summary>
    internal const string SearchQuery = "преключиться вдруг верный друг";


    // public const string SearchQuery = "пляшем на столе за детей";
    // public const string SearchQuery = "приключится вдруг верный друг";
}
