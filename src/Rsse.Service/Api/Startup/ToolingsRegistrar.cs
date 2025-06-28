using Microsoft.Extensions.DependencyInjection;
using Rsse.Api.Services;
using Rsse.Tooling.Contracts;
using Rsse.Tooling.MigrationAssistant;

namespace Rsse.Api.Startup;

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
