using System;

namespace SearchEngine.Data.Configuration;

/// <summary>
/// Настройки доступа к данным.
/// </summary>
public class DatabaseOptions
{
    /// <summary>
    /// Выбор контекста бд для чтения.
    /// </summary>
    public DatabaseType ReaderContext { get; init; }

    /// <summary>
    /// Применять ли скрипт с созданием таблиц на миграции для postgres.
    /// </summary>
    [Obsolete("Не используется.")]
    public bool CreateTablesOnPgMigration { get; init; }
}
