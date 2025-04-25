using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using SearchEngine.Controllers;
using SearchEngine.Data.Context;
using SearchEngine.Data.Repository;

namespace SearchEngine.Tests.Integrations.Infra;

/// <summary>
/// Расширение функционала регистрации служб, для целей тестирования.
/// </summary>
public static class TestConfigurationExtensions
{
    /// <summary>
    /// Зарегистрировать контроллеры и провайдеры для тестовых бд.
    /// </summary>
    /// <param name="services">коллекция служб</param>
    internal static void AddTestEnvironment(this IServiceCollection services)
    {
        // todo разберись почему требуется AddApplicationPart:
        // https://andrewlock.net/when-asp-net-core-cant-find-your-controller-debugging-application-parts/

        services
            .AddControllers()
            .AddApplicationPart(typeof(ReadController).Assembly);

        // SQLite: https://learn.microsoft.com/en-us/ef/core/get-started/overview/first-app?tabs=netcore-cli
        // функциональность проверена на Windows/Ubuntu:

        const Environment.SpecialFolder folder = Environment.SpecialFolder.LocalApplicationData;
        var path = Environment.GetFolderPath(folder);
        var dbPath = System.IO.Path.Join(path, "mysql.db");
        var mysqlConnectionString = $"Data Source={dbPath}";
        dbPath = System.IO.Path.Join(path, "postgres.db");
        var npgConnectionString = $"Data Source={dbPath}";

        services.AddDbContext<MysqlCatalogContext>(options =>
        {
            options.UseSqlite(mysqlConnectionString);
            options.EnableSensitiveDataLogging();
        });
        // для резолва CatalogRepository также регистрируем контекст postgres с базой данных Sqllite
        services.AddDbContext<NpgsqlCatalogContext>(options =>
        {
            options.UseSqlite(npgConnectionString);
            options.EnableSensitiveDataLogging();
        });

        services.AddScoped<CatalogRepository<MysqlCatalogContext>>();
        services.AddScoped<CatalogRepository<NpgsqlCatalogContext>>();
    }
}
