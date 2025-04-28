using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using SearchEngine.Common.Auth;

namespace SearchEngine.Tests.Integrations.Infra;

internal class CustomWebAppFactory<T> : WebApplicationFactory<T> where T : class
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
        // runs on pipelined services
        if (Docker.IsGitHubAction())
        {
            Console.WriteLine($"{nameof(GetMysqlConnectionString)} | github action | host '{Docker.MySqlHost}'");
            return $"Server={Docker.MySqlHost};Database=tagit;Uid=root;Pwd=1;Port={Docker.MySqlPort}";
        }

        // runs locally on docker
        if (IsIntegration()) return $"Server=127.0.0.1;Database=tagit;Uid=root;Pwd=1;Port={Docker.MySqlPort}";

        // runs locally on sqlite
        const Environment.SpecialFolder folder = Environment.SpecialFolder.LocalApplicationData;
        var path = Environment.GetFolderPath(folder);
        var mysqlDbPath = Path.Join(path, $"mysql-{Guid.NewGuid()}.db");
        var mysqlConnectionString = $"Data Source={mysqlDbPath}";


        return mysqlConnectionString;
    }

    private static string GetPgConnectionString()
    {
        // runs on pipelined services
        if (Docker.IsGitHubAction()) {
            Console.WriteLine($"{nameof(GetPgConnectionString)} | github action | host '{Docker.PostgresHost}'");
            return $"Include Error Detail=true;Server={Docker.PostgresHost};Database=tagit;Port={Docker.PostgresPort};" +
                                            $"Userid=1;Password=1;Pooling=false;MinPoolSize=1;MaxPoolSize=20;Timeout=15;SslMode=Disable";}

        // runs locally on docker
        if (IsIntegration()) return $"Include Error Detail=true;Server=127.0.0.1;Database=tagit;Port={Docker.PostgresPort};" +
                                    $"Userid=1;Password=1;Pooling=false;MinPoolSize=1;MaxPoolSize=20;Timeout=15;SslMode=Disable";

        // runs locally on sqlite
        const Environment.SpecialFolder folder = Environment.SpecialFolder.LocalApplicationData;
        var path = Environment.GetFolderPath(folder);
        var pgDbPath = Path.Join(path, $"postgres-{Guid.NewGuid()}.db");
        var npgConnectionString = $"Data Source={pgDbPath}";

        return npgConnectionString;
    }

    private static bool IsIntegration() => typeof(T) == typeof(IntegrationMirrorStartup);
}
