using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SearchEngine.Configuration;
using SearchEngine.Controllers;
using SearchEngine.Data;
using SearchEngine.Data.Repository;
using SearchEngine.Data.Repository.Contracts;
using SearchEngine.Infrastructure.Engine;
using SearchEngine.Infrastructure.Tokenizer;
using SearchEngine.Infrastructure.Tokenizer.Contracts;
using SearchEngine.Tests.Infrastructure;

namespace SearchEngine.Tests.Integration;

public class IntegrationStartup
{
    public static void ConfigureServices(IServiceCollection services)
    {
        services
            .AddControllers()
            // разберись почему требуется этот метод:
            // https://andrewlock.net/when-asp-net-core-cant-find-your-controller-debugging-application-parts/
            .AddApplicationPart(typeof(ReadController).Assembly);

        services.AddTransient<IDataRepository, DataRepository>();
        services.Configure<CommonBaseOptions>(options => options.TokenizerIsEnable = true);

        services.AddSingleton<ILogger, TestLogger<IntegrationStartup>>();
        services.AddTransient<ITokenizerProcessor, TokenizerProcessor>();
        services.AddTransient<ITokenizerService, TokenizerService>();

        // III. SQLITE: https://learn.microsoft.com/en-us/ef/core/get-started/overview/first-app?tabs=netcore-cli
        // возможно, что данная папка есть только в windows:

        const Environment.SpecialFolder folder = Environment.SpecialFolder.LocalApplicationData;
        var path = Environment.GetFolderPath(folder);
        var dbPath = System.IO.Path.Join(path, "testing-2.db");
        var connectionString = $"Data Source={dbPath}";
        // https://www.sqlite.org/lang.html

        services.AddDbContext<RsseContext>(options => options.UseSqlite(connectionString));
    }

    public static void Configure(IApplicationBuilder app)
    {
        app.UseRouting();
        app.UseEndpoints(ep => ep.MapControllers());
    }
}
