using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SearchEngine.Api.Controllers;
using SearchEngine.Api.Startup;
using SearchEngine.Infrastructure.Context;
using SearchEngine.Infrastructure.Repository;

namespace SearchEngine.Tests.Integrations.Extensions;

/// <summary>
/// Расширение функционала регистрации служб для тестов с использованием sqlite.
/// </summary>
public static class TestConfigurationExtensions
{
    /// <summary>
    /// Зарегистрировать контроллеры и провайдеры для тестовых бд, используется SQLite.
    /// https://learn.microsoft.com/en-us/ef/core/get-started/overview/first-app?tabs=netcore-cli
    /// </summary>
    internal static void AddSqliteTestEnvironment(this IServiceCollection services)
    {
        // todo: разберись, почему требуется AddApplicationPart:
        // https://andrewlock.net/when-asp-net-core-cant-find-your-controller-debugging-application-parts/

        services
            .AddControllers()
            .AddApplicationPart(typeof(ReadController).Assembly);

        services.AddDbContext<MysqlCatalogContext>((sp, options) =>
        {
            var config = sp.GetRequiredService<IConfiguration>();
            var mysqlConnectionString = config.GetConnectionString(Startup.DefaultConnectionKey);
            options.UseSqlite(mysqlConnectionString);
        });

        services.AddDbContext<NpgsqlCatalogContext>((sp, options) =>
        {
            // для резолва CatalogRepository также регистрируем контекст postgres с базой данных SqLite
            var config = sp.GetRequiredService<IConfiguration>();
            var npgConnectionString = config.GetConnectionString(Startup.AdditionalConnectionKey);
            options.UseSqlite(npgConnectionString);
        });

        services.AddScoped<CatalogRepository<MysqlCatalogContext>>();
        services.AddScoped<CatalogRepository<NpgsqlCatalogContext>>();
    }
}
