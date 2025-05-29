using SearchEngine.Data.Configuration;

namespace SearchEngine.Tooling.Contracts;

/// <summary>
/// Контракт фабрики по созданию миграторов.
/// </summary>
public interface IDbMigratorFactory
{
    /// <summary>
    /// Создать мигратор для бд требуемого типа.
    /// </summary>
    /// <param name="databaseType">Тир бд для мигратора.</param>
    /// <returns></returns>
    IDbMigrator CreateMigrator(DatabaseType databaseType);
}
