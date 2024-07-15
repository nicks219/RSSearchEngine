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
internal class SimpleStartup
{
    public static void ConfigureServices(IServiceCollection services)
    {
        services.PartialConfigureForTesting();

        services.AddTransient<IDataRepository, CatalogRepository>();
        services.Configure<CommonBaseOptions>(options => options.TokenizerIsEnable = true);

        services.AddSingleton<ILogger, TestLogger<SimpleStartup>>();
        services.AddTransient<ITokenizerProcessor, TokenizerProcessor>();
        services.AddTransient<ITokenizerService, TokenizerService>();
    }

    public static void Configure(IApplicationBuilder app, ILoggerFactory loggerFactory)
    {
        app.UseRouting();
        app.UseEndpoints(ep => ep.MapControllers());
    }
}
