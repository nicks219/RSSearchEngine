using System;
using System.Collections.Generic;
using System.Linq;
using Rsse.Domain.Data.Configuration;
using Rsse.Tooling.Contracts;

namespace Rsse.Tooling.MigrationAssistant;

/// <summary>
/// Фабрика по созданию миграторов.
/// </summary>
public class MigratorFactory : IDbMigratorFactory
{
    private readonly IDbMigrator _mySqlMigrator;
    private readonly IDbMigrator _postgresMigrator;

    /// <summary>
    /// Закэшировать миграторы.
    /// </summary>
    /// <param name="migrators">Коллекция миграторов из DI.</param>
    public MigratorFactory(IEnumerable<IDbMigrator> migrators)
    {
        var dbMigrators = migrators.ToList();
        _mySqlMigrator = dbMigrators.First(m => m.GetType() == typeof(MySqlDbMigrator));
        _postgresMigrator = dbMigrators.First(m => m.GetType() == typeof(NpgsqlDbMigrator));
    }

    /// <inheritdoc/>
    /// <exception cref="ArgumentOutOfRangeException">Неизвестный тип бд.</exception>
    public IDbMigrator CreateMigrator(DatabaseType databaseType)
    {
        var migrator = databaseType switch
        {
            DatabaseType.MySql => _mySqlMigrator,
            DatabaseType.Postgres => _postgresMigrator,
            _ => throw new ArgumentOutOfRangeException(nameof(databaseType), databaseType, "unknown database type")
        };

        return migrator;
    }
}
