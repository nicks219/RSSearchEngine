using Microsoft.AspNetCore.Server.Kestrel.Core;
using RandomSongSearchEngine;

// TODO: переименуй с учетом, что речь идёт не о "песнях", а о "текстах" или "заметках".
// 1. нейминг в программе.
// 2. async-await в JS.
// 3. нейминг в DTO и сущностях бд.

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
            
            // options.Listen(new IPEndPoint(new IPAddress(new byte[]{127,0,0,1}), 5000));
        });
    });

var app = builder.Build();

try
{
    app.Run();
}
catch (InvalidOperationException ex)
{
    throw new Exception("[DB FAILURE] turn on database server please", ex);
}