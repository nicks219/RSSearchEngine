using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Rsse.Api.Startup;
using Rsse.Domain.Service.Configuration;
using Rsse.Tests.Integration.RealDb.Infra;
using Serilog;

namespace Rsse.Tests.Integration.RealDb.Api;

public class IntegrationWebAppFactory<T> : WebApplicationFactory<T> where T : Startup
{
    protected override IHostBuilder CreateHostBuilder()
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Warning()
            .WriteTo.Console()
            .CreateLogger();

        var mysqlConnectionString = GetMysqlConnectionString();
        var pgConnectionString = GetPgConnectionString();

        var initialData = new Dictionary<string, string?>
        {
            ["ConnectionStrings:DefaultConnection"] = mysqlConnectionString,
            ["ConnectionStrings:AdditionalConnection"] = pgConnectionString,
            ["CommonBaseOptions:CreateBackupForNewSong"] = "true"
        };

        var builder = Host.CreateDefaultBuilder()
            .ConfigureWebHostDefaults(webBuilder =>
            {
                // TestingEnvironment | ProductionEnvironment
                Environment.SetEnvironmentVariable(Constants.AspNetCoreEnvironmentName, Constants.ProductionEnvironment);
                Environment.SetEnvironmentVariable(Constants.AspNetCoreOtlpExportersDisable, Constants.DisableValue);
                webBuilder.UseStartup<T>();
            })
            .ConfigureAppConfiguration((_, config) =>
            {
                config.AddInMemoryCollection(initialData);
            })
            .ConfigureServices(services =>
            {
                if (EnvironmentReporter.IsTesting())
                {
                    // не вызывать, если тест запущен для production тк сервисы подключатся в startup
                    services.AddDbsIntegrationTestEnvironment();
                }
            });

        builder.UseSerilog();
        return builder;
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder.UseContentRoot(Directory.GetCurrentDirectory());
        var host = base.CreateHost(builder);
        return host;
    }

    private static string GetMysqlConnectionString()
    {
        // integrations: runs on pipelined services or runs locally on docker
        var host = Docker.IsGitHubAction() ? Docker.MySqlHostFromGitHub : Docker.Localhost;
        Console.WriteLine($"{nameof(GetMysqlConnectionString)} | host '{host}:{Docker.MySqlPort}'");

        return $"Server={host};Database=tagit;Uid=root;Pwd=1;Port={Docker.MySqlPort};" +
               $"AllowUserVariables=True;UseAffectedRows=False";
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

