using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SearchEngine.Controllers;
using SearchEngine.Data.Context;
using SearchEngine.Data.Repository;
using SearchEngine.Tools.MigrationAssistant;

namespace SearchEngine.Tests.Integrations.Infra;

/// <summary>
/// Расширение функционала регистрации служб, для целей тестирования.
/// </summary>
public static class TestConfigurationExtensions
{
    /// <summary>
    /// Зарегистрировать контроллеры и провайдеры для тестовых бд, используется sqlite.
    /// </summary>
    internal static void AddSqliteTestEnvironment(this IServiceCollection services)
    {
        // todo разберись почему требуется AddApplicationPart:
        // https://andrewlock.net/when-asp-net-core-cant-find-your-controller-debugging-application-parts/

        services
            .AddControllers()
            .AddApplicationPart(typeof(ReadController).Assembly);

        // SQLite: https://learn.microsoft.com/en-us/ef/core/get-started/overview/first-app?tabs=netcore-cli
        // функциональность проверена на Windows/Ubuntu:

        services.AddDbContext<MysqlCatalogContext>((sp, options) =>
        {
            var config = sp.GetRequiredService<IConfiguration>();
            var mysqlConnectionString = config.GetConnectionString(Startup.DefaultConnectionKey);
            options.UseSqlite(mysqlConnectionString);
        });
        // для резолва CatalogRepository также регистрируем контекст postgres с базой данных Sqllite
        services.AddDbContext<NpgsqlCatalogContext>((sp, options) =>
        {
            var config = sp.GetRequiredService<IConfiguration>();
            var npgConnectionString = config.GetConnectionString(Startup.AdditionalConnectionKey);
            options.UseSqlite(npgConnectionString);
        });

        services.AddScoped<CatalogRepository<MysqlCatalogContext>>();
        services.AddScoped<CatalogRepository<NpgsqlCatalogContext>>();
    }

    /// <summary>
    /// Зарегистрировать контроллеры и провайдеры для тестовых бд, используются провайдеры до mysql и postgres.
    /// </summary>
    internal static void AddDbsTestEnvironment(this IServiceCollection services)
    {
        services.AddControllers().AddApplicationPart(typeof(ReadController).Assembly);

        var mySqlVersion = new MySqlServerVersion(new Version(8, 0, 31));
        services.AddDbContext<MysqlCatalogContext>((sp, options) =>
        {
            var config = sp.GetRequiredService<IConfiguration>();
            var mysqlConnectionString = config.GetConnectionString(Startup.DefaultConnectionKey);
            options.UseMySql(mysqlConnectionString, mySqlVersion);
            options.EnableSensitiveDataLogging();
        });
        services.AddDbContext<NpgsqlCatalogContext>((sp, options) =>
        {
            var config = sp.GetRequiredService<IConfiguration>();
            var npgConnectionString = config.GetConnectionString(Startup.AdditionalConnectionKey);
            options.UseNpgsql(npgConnectionString);
            options.EnableSensitiveDataLogging();
        });

        services.AddSingleton<IDbMigrator, MySqlDbMigrator>();
        services.AddSingleton<IDbMigrator, NpgsqlDbMigrator>();

        services.AddScoped<CatalogRepository<MysqlCatalogContext>>();
        services.AddScoped<CatalogRepository<NpgsqlCatalogContext>>();
    }
}
