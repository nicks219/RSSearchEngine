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
using SearchEngine.Api.Logger;
using SearchEngine.Api.Startup;
using SearchEngine.Service.Configuration;
using SearchEngine.Tooling.DevelopmentAssistant;
using Serilog;
using Serilog.Sinks.OpenTelemetry;

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

var env = Environment.GetEnvironmentVariable(Constants.AspNetCoreEnvironmentName) ?? Environments.Production;
var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json")
    .AddJsonFile($"appsettings.{env}.json", true)
    .Build();

var resourceAttributes = ResourceBuilder
    .CreateDefault()
    .AddService("rsse-app", serviceNamespace: "rsse-group", serviceVersion: Constants.ApplicationVersion)
    .Build().Attributes.ToDictionary();

Log.Logger = new LoggerConfiguration()
    .Enrich.With<ActivityEnricher>()
    .ReadFrom
    .Configuration(configuration)
    .WriteTo.OpenTelemetry(options =>
    {
        options.Endpoint = "http://otel-collector:4317";
        options.Protocol = OtlpProtocol.Grpc;
        options.ResourceAttributes = resourceAttributes;
    })
    .CreateLogger();

Log.ForContext<Program>().Information("Starting web host");

try
{
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
