using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using SearchEngine.Common.Configuration;
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
        // для резолва CatalogRepository также регистрируем контекст для postgres (использует Sqllite для инициализации)
        services.AddDbContext<NpgsqlCatalogContext>(options => options.UseSqlite(connectionString));
    }

    public static void Configure(IApplicationBuilder app, ILoggerFactory loggerFactory)
    {
        app.UseRouting();
        app.UseEndpoints(ep => ep.MapControllers());
    }
}
