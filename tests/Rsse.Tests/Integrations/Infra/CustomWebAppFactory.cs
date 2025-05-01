using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using SearchEngine.Common;
using SearchEngine.Common.Auth;

namespace SearchEngine.Tests.Integrations.Infra;

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
            // .ConfigureServices((ctx, services) => services.AddSingleton<IConfiguration>(provider => ctx.Configuration))
            .ConfigureWebHostDefaults(webBuilder =>
            {
                Environment.SetEnvironmentVariable(Constants.AspNetCoreEnvironmentName, Constants.TestingEnvironment);
                webBuilder.UseStartup<T>();
            });

        return builder;
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder.UseContentRoot(Directory.GetCurrentDirectory());
        var host = base.CreateHost(builder);
        HostInternal = host;
        return host;
    }

    private static string GetMysqlConnectionString()
    {
        // runs locally on sqlite
        if (!IsIntegration())
        {
            const Environment.SpecialFolder folder = Environment.SpecialFolder.LocalApplicationData;
            var path = Environment.GetFolderPath(folder);
            var mysqlDbPath = Path.Join(path, $"mysql-{Guid.NewGuid()}.db");
            var mysqlConnectionString = $"Data Source={mysqlDbPath}";
            SqliteFileCleaner.Store.Push(mysqlDbPath);
            Console.WriteLine($"{nameof(GetPgConnectionString)} | sqlite | {Path.GetFileName(mysqlDbPath)}");

            return mysqlConnectionString;
        }

        // integrations: runs on pipelined services or runs locally on docker
        var host = Docker.IsGitHubAction() ? Docker.MySqlHostFromGitHub : Docker.Localhost;
        Console.WriteLine($"{nameof(GetMysqlConnectionString)} | host '{host}:{Docker.MySqlPort}'");

        return $"Server={host};Database=tagit;Uid=root;Pwd=1;Port={Docker.MySqlPort}";
    }

    private static string GetPgConnectionString()
    {
        // runs locally on sqlite
        if (!IsIntegration())
        {
            const Environment.SpecialFolder folder = Environment.SpecialFolder.LocalApplicationData;
            var path = Environment.GetFolderPath(folder);
            var pgDbPath = Path.Join(path, $"postgres-{Guid.NewGuid()}.db");
            var npgConnectionString = $"Data Source={pgDbPath}";
            SqliteFileCleaner.Store.Push(pgDbPath);
            Console.WriteLine($"{nameof(GetPgConnectionString)} | sqlite | {Path.GetFileName(pgDbPath)}");

            return npgConnectionString;
        }

        // integrations: runs on pipelined services or runs locally on docker
        var host = Docker.IsGitHubAction() ? Docker.PostgresHostFromGitHub : Docker.Localhost;
        Console.WriteLine($"{nameof(GetMysqlConnectionString)} | host '{host}:{Docker.PostgresPort}'");

        return $"Include Error Detail=true;Server={host};Database=tagit;Port={Docker.PostgresPort};" +
               $"Userid=1;Password=1;Pooling=false;MinPoolSize=1;MaxPoolSize=20;Timeout=15;SslMode=Disable";
    }

    private static bool IsIntegration() => typeof(T) == typeof(IntegrationStartup);
}

