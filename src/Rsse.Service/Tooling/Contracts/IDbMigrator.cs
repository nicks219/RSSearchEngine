using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SearchEngine.Data.Configuration;
using SearchEngine.Tooling.MigrationAssistant;

namespace SearchEngine.Tooling.Contracts;

/// <summary>
/// Контракт функционала работы с миграциями бд.
/// </summary>
public interface IDbMigrator
{
    /// <summary>
    /// Создать миграцию.
    /// </summary>
    /// <param name="fileName">Имя создаваемого файла миграции, при пустом аргументе используются имена из ротации.</param>
    /// <returns>Путь к созданному файлу миграции.</returns>
    public string Create(string? fileName);

    /// <summary>
    /// Применить миграцию.
    /// </summary>
    /// <param name="fileName">Имя существующего файла миграции, при пустом аргументе используются имена из ротации.</param>
    /// <returns>Путь к существующему файлу миграции.</returns>
    public string Restore(string? fileName);

    /// <summary>
    /// Скопировать данные из MySql в Postgres.
    /// </summary>
    // todo: удалить из контракта после завершения перехода на Postgres
    public Task CopyDbFromMysqlToNpgsql();

    /// <summary>
    /// Получить мигратор требуемого типа из списка зависимостей.
    /// </summary>
    internal static IDbMigrator GetMigrator(IEnumerable<IDbMigrator> migrators, DatabaseType databaseType)
    {
        var migrator = databaseType switch
        {
            DatabaseType.MySql => migrators.First(m => m.GetType() == typeof(MySqlDbMigrator)),
            DatabaseType.Postgres => migrators.First(m => m.GetType() == typeof(NpgsqlDbMigrator)),
            _ => throw new ArgumentOutOfRangeException(nameof(databaseType), databaseType, "unknown database type")
        };

        return migrator;
    }
}
