using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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
    /// <param name="ct">Токен отмены.</param>
    /// <returns>Путь к созданному файлу миграции.</returns>
    public Task<string> Create(string? fileName, CancellationToken ct);

    /// <summary>
    /// Применить миграцию.
    /// </summary>
    /// <param name="fileName">Имя существующего файла миграции, при пустом аргументе используются имена из ротации.</param>
    /// <param name="ct">Токен отмены.</param>
    /// <returns>Путь к существующему файлу миграции.</returns>
    public Task<string> Restore(string? fileName, CancellationToken ct);

    /// <summary>
    /// Скопировать данные из MySql в Postgres.
    /// </summary>
    /// <param name="ct">Токен отмены.</param>
    // todo: удалить из контракта после завершения перехода на Postgres
    public Task CopyDbFromMysqlToNpgsql(CancellationToken ct);

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
