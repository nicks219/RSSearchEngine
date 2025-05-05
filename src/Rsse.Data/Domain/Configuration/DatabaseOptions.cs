namespace SearchEngine.Domain.Configuration;

/// <summary>
/// Настройки доступа к данным
/// </summary>
public class DatabaseOptions
{
    /// <summary>
    /// Выбор контекста бд для чтения.
    /// </summary>
    public DatabaseType ReaderContext { get; set; }

    /// <summary>
    /// Применять ли скрипт с созданием таблиц на миграции для postgres.
    /// </summary>
    public bool CreateTablesOnPgMigration { get; set; }
}
