namespace SearchEngine.Service.Configuration;

/// <summary>
/// Алгоритм выбора следующей заметки.
/// </summary>
public enum ElectionType
{
    SqlRandom = 0,
    Rng = 1,
    RoundRobin = 2,
    Unique = 3
}
