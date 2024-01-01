using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SearchEngine.Common.Configuration;
using SearchEngine.Controllers;
using SearchEngine.Data.Context;
using SearchEngine.Data.Repository;
using SearchEngine.Data.Repository.Contracts;
using SearchEngine.Engine.Contracts;
using SearchEngine.Engine.Tokenizer;
using SearchEngine.Tests.Units.Mocks;

namespace SearchEngine.Tests.Integrations.Infra;

/// <summary>
/// При конфинурации запуска используется SQLite, информация по данной бд: https://www.sqlite.org/lang.html
/// </summary>
internal class IntegrationStartup
{
    public static void ConfigureServices(IServiceCollection services)
    {
        services
            .AddControllers()
            // разберись почему требуется этот метод:
            // https://andrewlock.net/when-asp-net-core-cant-find-your-controller-debugging-application-parts/
            .AddApplicationPart(typeof(ReadController).Assembly);

        services.AddTransient<IDataRepository, CatalogRepository>();
        services.Configure<CommonBaseOptions>(options => options.TokenizerIsEnable = true);

        services.AddSingleton<ILogger, TestLogger<IntegrationStartup>>();
        services.AddTransient<ITokenizerProcessor, TokenizerProcessor>();
        services.AddTransient<ITokenizerService, TokenizerService>();

        // SQLite: https://learn.microsoft.com/en-us/ef/core/get-started/overview/first-app?tabs=netcore-cli
        // функциональность проверена на Windows/Ubuntu:

        const Environment.SpecialFolder folder = Environment.SpecialFolder.LocalApplicationData;
        var path = Environment.GetFolderPath(folder);
        var dbPath = System.IO.Path.Join(path, "testing-2.db");
        var connectionString = $"Data Source={dbPath}";

        services.AddDbContext<CatalogContext>(options => options.UseSqlite(connectionString));
    }

    public static void Configure(IApplicationBuilder app)
    {
        app.UseRouting();
        app.UseEndpoints(ep => ep.MapControllers());
    }
}
