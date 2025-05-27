using System.Threading;

namespace SearchEngine.Api.Services;

/// <summary>
/// Контейнер с состоянием миграторов.
/// </summary>
public class MigratorState
{
    private int _activeCount;

    /// <summary>
    /// Выполняется ли в данный момент хотя бы одна миграция.
    /// </summary>
    public bool IsMigrating => Volatile.Read(ref _activeCount) > 0;

    /// <summary>
    /// Миграция началась.
    /// </summary>
    public void Start() => Interlocked.Increment(ref _activeCount);

    /// <summary>
    /// Миграция завершилась.
    /// </summary>
    public void End() => Interlocked.Decrement(ref _activeCount);
}
