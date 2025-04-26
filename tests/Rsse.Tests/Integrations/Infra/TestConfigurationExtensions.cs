using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
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

        const Environment.SpecialFolder folder = Environment.SpecialFolder.LocalApplicationData;
        var path = Environment.GetFolderPath(folder);
        var mysqlDbPath = System.IO.Path.Join(path, "mysql.db");
        var mysqlConnectionString = $"Data Source={mysqlDbPath}";
        var pgDbPath = System.IO.Path.Join(path, "postgres.db");
        var npgConnectionString = $"Data Source={pgDbPath}";

        services.AddDbContext<MysqlCatalogContext>(options =>
        {
            options.UseSqlite(mysqlConnectionString);
        });
        // для резолва CatalogRepository также регистрируем контекст postgres с базой данных Sqllite
        services.AddDbContext<NpgsqlCatalogContext>(options =>
        {
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

        var mysqlConnectionString = $"Server=127.0.0.1;Database=tagit;Uid=root;Pwd=1;Port={Docker.MySqlPort}";
        var npgConnectionString = $"Include Error Detail=true;Server=127.0.0.1;Database=tagit;Port={Docker.PostgresPort};" +
                                  $"Userid=1;Password=1;Pooling=false;MinPoolSize=1;MaxPoolSize=20;Timeout=15;SslMode=Disable";

        var mySqlVersion = new MySqlServerVersion(new Version(8, 0, 31));
        services.AddDbContext<MysqlCatalogContext>(options =>
        {
            options.UseMySql(mysqlConnectionString, mySqlVersion);
            options.EnableSensitiveDataLogging();
        });
        services.AddDbContext<NpgsqlCatalogContext>(options =>
        {
            options.UseNpgsql(npgConnectionString);
            options.EnableSensitiveDataLogging();
        });

        services.AddSingleton<IDbMigrator, MySqlDbMigrator>();
        services.AddSingleton<IDbMigrator, NpgsqlDbMigrator>();

        services.AddScoped<CatalogRepository<MysqlCatalogContext>>();
        services.AddScoped<CatalogRepository<NpgsqlCatalogContext>>();
    }
}
