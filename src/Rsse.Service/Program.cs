using System;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using SearchEngine.Api.Startup;
using SearchEngine.Service.Configuration;
using SearchEngine.Tooling.DevelopmentAssistant;
using Serilog;

#if WINDOWS
var standaloneMode = ClientLauncher.Run(args);
if (standaloneMode) return 0;
#endif

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
Log.Logger = new LoggerConfiguration()
    .ReadFrom
    .Configuration(configuration)
    .CreateLogger();

Log.Information("Starting web host");

try
{
    var app = builder.Build();
    app.Run();
    return 0;
}
catch (InvalidOperationException ex)
{
    Log.Fatal(ex, "[{Reporter}] | startup error, more likely db server is down, see exception:\r\n{Message}",
        nameof(Program), ex.Message);
    return 1;
}
catch (Exception ex)
{
    Log.Fatal(ex, "[{Reporter}] | startup error, see exception:\r\n{Message}", nameof(Program), ex.Message);
    return 1;
}
finally
{
    Log.CloseAndFlush();
}
