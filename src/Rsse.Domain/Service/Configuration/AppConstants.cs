namespace SearchEngine.Service.Configuration;

/// <summary>
/// Константы приложения.
/// </summary>
public abstract class AppConstants
{
    /// <summary>
    /// Общее время ожидания завершения миграций при остановке хоста.
    /// </summary>
    public const int WaitMigratorTotalSeconds = 25;

    /// <summary>
    /// Пауза между проверками завершения миграций при их ожидании во время остановке хоста.
    /// </summary>
    public const int WaitMigratorNextCheckMs = 500;

    /// <summary>
    /// Минимальный ID тега.
    /// </summary>
    public const int MinTagNumber = 1;
}
