namespace SearchEngine.Domain.Configuration;

/// <summary>
/// Настройки доступа к данным.
/// </summary>
public class DatabaseOptions
{
    /// <summary>
    /// Выбор контекста бд для чтения.
    /// </summary>
    public MigratorType ReaderContext { get; init; }

    /// <summary>
    /// Применять ли скрипт с созданием таблиц на миграции для postgres.
    /// </summary>
    public bool CreateTablesOnPgMigration { get; init; }
}
