using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SearchEngine.Tests.Integrations.Infra;

namespace SearchEngine.Tests.Integrations.IntegrationTests.RealDb;

public class IntegrationWebAppFactory<T> : WebApplicationFactory<T> where T : class
{
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
                webBuilder.UseStartup<T>();
            })
            .ConfigureServices(TryReplaceStartupDatabaseProviders);

        return builder;
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder.UseContentRoot(Directory.GetCurrentDirectory());
        var host = base.CreateHost(builder);
        return host;
    }

    private static void TryReplaceStartupDatabaseProviders(IServiceCollection services) { }

    private static string GetMysqlConnectionString()
    {
        // integrations: runs on pipelined services or runs locally on docker
        var host = Docker.IsGitHubAction() ? Docker.MySqlHostFromGitHub : Docker.Localhost;
        Console.WriteLine($"{nameof(GetMysqlConnectionString)} | host '{host}:{Docker.MySqlPort}'");

        return $"Server={host};Database=tagit;Uid=root;Pwd=1;Port={Docker.MySqlPort};AllowUserVariables=True;UseAffectedRows=False";
    }

    private static string GetPgConnectionString()
    {
        // integrations: runs on pipelined services or runs locally on docker
        var host = Docker.IsGitHubAction() ? Docker.PostgresHostFromGitHub : Docker.Localhost;
        Console.WriteLine($"{nameof(GetMysqlConnectionString)} | host '{host}:{Docker.PostgresPort}'");

        return $"Include Error Detail=true;Server={host};Database=tagit;Port={Docker.PostgresPort};" +
               $"Userid=1;Password=1;Pooling=false;MinPoolSize=1;MaxPoolSize=20;Timeout=15;SslMode=Disable";
    }
}

