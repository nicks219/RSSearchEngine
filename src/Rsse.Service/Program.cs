using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Hosting;
using SearchEngine;
#if WINDOWS
using SearchEngine.ClientDevelopmentIntegration;

var standaloneMode = ClientLauncher.Run(args);
if (standaloneMode) return;
#endif

var builder = Host.CreateDefaultBuilder(args)
    .ConfigureWebHostDefaults(webBuilder =>
    {
        webBuilder.UseStartup<Startup>();
        webBuilder.UseWebRoot("ClientApp/build");
        webBuilder.UseKestrel(options =>
        {
            // defaults:
            // MinRequestBodyDataRate = 240b/s grace 00:00:05
            // MinResponseDataRate = 240b/s grace 00:00:05
            // MaxConcurrentConnections = null

            var kestrelLimits = options.Limits;

            kestrelLimits.MinResponseDataRate = new MinDataRate(100, TimeSpan.FromSeconds(5));

            kestrelLimits.MinRequestBodyDataRate = new MinDataRate(100, TimeSpan.FromSeconds(5));
        });
    });

var app = builder.Build();

try
{
    app.Run();
}
catch (InvalidOperationException ex)
{
    throw new Exception("[STARTUP ERROR] more likely db server is down", ex);
}