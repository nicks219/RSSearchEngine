using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MySqlConnector;
using Npgsql;
using SearchEngine.Api.Startup;
using SearchEngine.Infrastructure.Context;
using SearchEngine.Service.Configuration;
using SearchEngine.Tests.Integrations.Extensions;
using SearchEngine.Tests.Integrations.Infra;

namespace SearchEngine.Tests.Integrations.Api;

public class CustomWebAppFactory<T> : WebApplicationFactory<T> where T : class
{
    internal IHost? HostInternal { get; private set; }

    protected override IHostBuilder CreateHostBuilder()
    {
        var mysqlConnectionString = GetMysqlConnectionString();
        var pgConnectionString = GetPgConnectionString();

        var initialData = new Dictionary<string, string?>
        {
            ["ConnectionStrings:DefaultConnection"] = mysqlConnectionString,
            ["ConnectionStrings:AdditionalConnection"] = pgConnectionString,
        };

        var builder = Host.CreateDefaultBuilder()
            .ConfigureAppConfiguration((_, config) =>
            {
                config.AddInMemoryCollection(initialData);
            })
            .ConfigureWebHostDefaults(webBuilder =>
            {
                Environment.SetEnvironmentVariable(Constants.AspNetCoreEnvironmentName, Constants.TestingEnvironment);
                webBuilder.UseStartup<T>();
            })
            .ConfigureServices(TryReplaceStartupDatabaseProviders);

        return builder;
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder.UseContentRoot(Directory.GetCurrentDirectory());
        var host = base.CreateHost(builder);
        HostInternal = host;
        return host;
    }

    // заменить оригинальную регистрацию контекстов на тестовую для Startup сервиса
    private static void TryReplaceStartupDatabaseProviders(IServiceCollection services)
    {
        // ApiAccessControlTests использует Startup сервиса
        if (typeof(T) != typeof(Startup)) return;

        // в данной версии регистрация этих зависимостей закрыта в Startup переменной окружения:
        // TryRemoveDependencies(services);

        const string postgresConnectionStringMock = "Include Error Detail=true;Server=127.0.0.1;Database=tagit;Port=5433;" +
                                    "Userid=1;Password=1;Pooling=false;MinPoolSize=1;MaxPoolSize=20;Timeout=15;SslMode=Disable";
        var npgDataSourceMock = new NpgsqlDataSourceBuilder(postgresConnectionStringMock).Build();
        services.AddSingleton(npgDataSourceMock);
        services.AddSqliteTestEnvironment();
    }

    // попытаться удалить зависимости для контекстов бд
    private static void TryRemoveDependencies(IServiceCollection services)
    {
        var types = new List<Type>
        {
            typeof(DbContextOptions<MysqlCatalogContext>),
            typeof(DbContextOptions<NpgsqlCatalogContext>),
            typeof(MysqlCatalogContext),
            typeof(NpgsqlCatalogContext),
            typeof(IDbContextOptionsConfiguration<MysqlCatalogContext>),
            typeof(IDbContextOptionsConfiguration<NpgsqlCatalogContext>),
            // datasources:
            typeof(MySqlDataSource),
            typeof(NpgsqlDataSource)
        };

        foreach (var descriptor in types.Select(type => services.FirstOrDefault(d => d.ServiceType == type)).OfType<ServiceDescriptor>())
        {
            services.Remove(descriptor);
        }
    }

    private static string GetMysqlConnectionString()
    {
        // runs locally on sqlite:
        const Environment.SpecialFolder folder = Environment.SpecialFolder.LocalApplicationData;
        var path = Environment.GetFolderPath(folder);
        var mysqlDbPath = Path.Join(path, $"mysql-{Guid.NewGuid()}.db");
        var mysqlConnectionString = $"Data Source={mysqlDbPath}";
        SqliteFileCleaner.Store.Push(mysqlDbPath);
        Console.WriteLine($"{nameof(GetPgConnectionString)} | sqlite | {Path.GetFileName(mysqlDbPath)}");

        return mysqlConnectionString;
    }

    private static string GetPgConnectionString()
    {
        // runs locally on sqlite:
        const Environment.SpecialFolder folder = Environment.SpecialFolder.LocalApplicationData;
        var path = Environment.GetFolderPath(folder);
        var pgDbPath = Path.Join(path, $"postgres-{Guid.NewGuid()}.db");
        var npgConnectionString = $"Data Source={pgDbPath}";
        SqliteFileCleaner.Store.Push(pgDbPath);
        Console.WriteLine($"{nameof(GetPgConnectionString)} | sqlite | {Path.GetFileName(pgDbPath)}");

        return npgConnectionString;
    }
}

