using Microsoft.Extensions.DependencyInjection;
using SearchEngine.Api.Services;
using SearchEngine.Tooling.Contracts;
using SearchEngine.Tooling.MigrationAssistant;

namespace SearchEngine.Api.Startup;

/// <summary>
/// Расширение, регистрирующее функционал тулинга.
/// </summary>
public static class ToolingsRegistrar
{
    /// <summary>
    /// Добавить зависимости для тулинга в DI.
    /// </summary>
    /// <param name="services"></param>
    public static void AddToolingDependencies(this IServiceCollection services)
    {
        services.AddSingleton<IDbMigratorFactory, MigratorFactory>();
        services.AddSingleton<IDbMigrator, MySqlDbMigrator>();
        services.AddSingleton<IDbMigrator, NpgsqlDbMigrator>();
        services.AddSingleton<MigratorState>();
    }
}
