using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Npgsql;
using SearchEngine.Infrastructure.Context;
using SearchEngine.Service.Configuration;

namespace SearchEngine.Api.Startup;

/// <summary>
/// Расширение для регистрации требуемых хранилищ данных.
/// </summary>
public static class CatalogDbExtensions
{
    private static readonly ServerVersion MySqlVersion = new MySqlServerVersion(new Version(8, 0, 31));

    /// <summary>
    /// Зарегистрировать все необходимые хранилища данных для всех окружений, кроме тестового.
    /// </summary>
    /// <param name="services">Коллекция служб.</param>
    /// <param name="configuration">Контейнер конфигурации.</param>
    /// <param name="env">Окружение веб-приложения.</param>
    public static void TryAddCatalogStores(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment env)
    {
        if (env.EnvironmentName == Constants.TestingEnvironment) return;

        var npgsqlConnectionString = configuration.GetConnectionString(Startup.AdditionalConnectionKey);
        var mysqlConnectionString = configuration.GetConnectionString(Startup.DefaultConnectionKey);
        if (string.IsNullOrEmpty(mysqlConnectionString) || string.IsNullOrEmpty(npgsqlConnectionString))
        {
            throw new NullReferenceException("Invalid connection string");
        }

        services.AddSingleton(sp =>
        {
            var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
            var npgsqlDataSource = new NpgsqlDataSourceBuilder(npgsqlConnectionString)
                .UseLoggerFactory(loggerFactory)
                .Build();

            return npgsqlDataSource;
        });

        services.AddDbContext<NpgsqlCatalogContext>((sp, options) =>
        {
            // логирование data source не зависит от environment
            var npgsqlDataSource = sp.GetRequiredService<NpgsqlDataSource>();
            options.UseNpgsql(npgsqlDataSource);
            options.UseLoggerFactory(NullLoggerFactory.Instance);
        });

        services.AddDbContext<MysqlCatalogContext>(options =>
        {
            options.UseMySql(mysqlConnectionString, MySqlVersion);
        });
    }
}
