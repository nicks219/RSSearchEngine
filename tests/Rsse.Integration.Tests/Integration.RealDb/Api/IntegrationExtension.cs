using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using MySqlConnector;
using Npgsql;
using SearchEngine.Api.Controllers;
using SearchEngine.Api.Startup;
using SearchEngine.Infrastructure.Context;
using SearchEngine.Infrastructure.Repository;
using SearchEngine.Tooling.Contracts;
using SearchEngine.Tooling.MigrationAssistant;
using Serilog;

namespace SearchEngine.Tests.Integration.RealDb.Api;

/// <summary>
/// Расширение функционала регистрации служб для тестов на контейнеризованных бд.
/// </summary>
public static class IntegrationExtension
{
    /// <summary>
    /// Зарегистрировать контроллеры и провайдеры для тестовых бд, используются провайдеры до mysql и postgres.
    /// </summary>
    internal static void AddDbsIntegrationTestEnvironment(this IServiceCollection services)
    {
        // register data source:
        var testLoggerFactory = LoggerFactory.Create(builder =>
        {
            // если требуются логи SQL на тестах:
            // builder.AddConsole();
            builder.AddSerilog();
        });

        services.AddSingleton<NpgsqlDataSource>(sp =>
        {
            var config = sp.GetRequiredService<IConfiguration>();
            var npgConnectionString = config.GetConnectionString(Startup.AdditionalConnectionKey);

            var npgsqlDataSource = new NpgsqlDataSourceBuilder(npgConnectionString)
                .UseLoggerFactory(testLoggerFactory)
                .Build();
            return npgsqlDataSource;
        });

        services.AddSingleton<MySqlDataSource>(sp =>
        {
            var config = sp.GetRequiredService<IConfiguration>();
            var mysqlConnectionString = config.GetConnectionString(Startup.DefaultConnectionKey);

            var mySqlDataSource = new MySqlDataSourceBuilder(mysqlConnectionString)
                .UseLoggerFactory(testLoggerFactory)
                .Build();

            return mySqlDataSource;
        });

        // register databases:
        services.AddDbContext<NpgsqlCatalogContext>((sp, options) =>
        {
            var npgSqlDataSource = sp.GetRequiredService<NpgsqlDataSource>();
            options.UseNpgsql(npgSqlDataSource);
            options.UseLoggerFactory(NullLoggerFactory.Instance);
        });

        var mySqlVersion = new MySqlServerVersion(new Version(8, 0, 31));
        services.AddDbContext<MysqlCatalogContext>((sp, options) =>
        {
            var mySqlDataSource = sp.GetRequiredService<MySqlDataSource>();
            options.UseMySql(mySqlDataSource, mySqlVersion);
            options.UseLoggerFactory(NullLoggerFactory.Instance);
            // переполнение кэша EF ServiceProviderCache на тестах в итоге вызовет InvalidOperationException:
            options.EnableServiceProviderCaching(false);
        });

        // other dependencies:
        services.AddControllers().AddApplicationPart(typeof(ReadController).Assembly);

        services.AddSingleton<IDbMigrator, MySqlDbMigrator>();
        services.AddSingleton<IDbMigrator, NpgsqlDbMigrator>();

        services.AddScoped<CatalogRepository<MysqlCatalogContext>>();
        services.AddScoped<CatalogRepository<NpgsqlCatalogContext>>();
    }
}
