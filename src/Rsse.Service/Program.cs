using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry.Resources;
using Rsse.Api.Observability;
using Rsse.Api.Startup;
using Rsse.Domain.Service.Configuration;
using Rsse.Tooling.DevelopmentAssistant;
using Serilog;
using Serilog.Sinks.OpenTelemetry;

namespace Rsse;

public class Program
{
    /// <summary>
    /// Собрать и запустить конфигурацию сервиса для прома/отладки.
    /// </summary>
    /// <param name="args">Аргументы командной строки.</param>
    public static async Task<int> Main(string[] args)
    {
#if WINDOWS
        var standaloneMode = ClientLauncher.Run(args);
        if (standaloneMode) return 0;
#endif

        TaskScheduler.UnobservedTaskException += (_, e) =>
        {
            Log.Error("{Reporter} | unobserved:\r\n{Exception}", nameof(Program), e.Exception);
            e.SetObserved();
        };

        AppDomain.CurrentDomain.UnhandledException += (_, e) =>
        {
            var ex = e.ExceptionObject as Exception;
            Log.Error("{Reporter} | unhandled:\r\n{Exception}", nameof(Program), ex);
        };

        var builder = CreateDefaultBuilder(args);

        UseSerilog();

        return await StartService(builder);
    }

    /// <summary>
    /// Сконфигурировать билдер хоста.
    /// </summary>
    /// <param name="args">Аргументы командной строки.</param>
    private static IHostBuilder CreateDefaultBuilder(string[] args)
    {
        var builder = Host.CreateDefaultBuilder(args)
            .UseSerilog()
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>();
                webBuilder.UseWebRoot(Constants.StaticDirectory);
                webBuilder.UseKestrel(options =>
                {
                    var kestrelLimits = options.Limits;
                    kestrelLimits.MinResponseDataRate = new MinDataRate(100, TimeSpan.FromSeconds(5));
                    kestrelLimits.MinRequestBodyDataRate = new MinDataRate(100, TimeSpan.FromSeconds(5));
                });
            });

        return builder;
    }

    /// <summary>
    /// Активировать использование Serilog.
    /// </summary>
    private static void UseSerilog()
    {
        var env = Environment.GetEnvironmentVariable(Constants.AspNetCoreEnvironmentName) ?? Environments.Production;
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .AddJsonFile($"appsettings.{env}.json", true)
            .Build();

        var resourceAttributes = ResourceBuilder
            .CreateDefault()
            .AddService(serviceName: Constants.ServiceName,
                serviceNamespace: Constants.ServiceNamespace,
                serviceVersion: Constants.ApplicationVersion)
            .Build().Attributes.ToDictionary();

        var otlpEndpoint = configuration.GetValue<string>("Otlp:Endpoint");
        if (string.IsNullOrEmpty(otlpEndpoint))
        {
            throw new Exception("Otlp:Endpoint not found.");
        }

        Log.Logger = new LoggerConfiguration()
            .Enrich.With<ActivityEnricher>()
            .ReadFrom
            .Configuration(configuration)
            .Filter.ByExcluding(log =>
                log.Properties.ContainsKey("RequestPath") &&
                (log.Properties["RequestPath"].ToString().StartsWith("\"/system")
                 || log.Properties["RequestPath"].ToString().StartsWith("\"/v6/account")))
#if TRACING_ENABLE
            .WriteTo.OpenTelemetry(options =>
            {
                options.Endpoint = otlpEndpoint;
                options.Protocol = OtlpProtocol.Grpc;
                options.ResourceAttributes = resourceAttributes;
            })
#endif
            .CreateLogger();
    }

    /// <summary>
    /// Запустить сервис.
    /// </summary>
    private static async Task<int> StartService(IHostBuilder builder)
    {
        try
        {
            Log.ForContext<Program>().Information("Starting web host");
            var app = builder.Build();
            var lifeTime = app.Services.GetRequiredService<IHostApplicationLifetime>();
            await app.RunAsync(lifeTime.ApplicationStopping);
            return 0;
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "[{Reporter}] | startup error:\r\n{Message}", nameof(Program), ex.Message);
            return 1;
        }
        finally
        {
            Log.ForContext<Program>().Information("Web host stopped...");
            var sw = Stopwatch.StartNew();
            await Log.CloseAndFlushAsync();
            Console.WriteLine($"Serilog closed for `{sw.Elapsed.TotalSeconds:F2}` sec...");
        }
    }
}
