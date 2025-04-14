using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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
    private static IConfiguration? _configuration;

    public SimpleStartup(IConfiguration configuration) => _configuration = configuration;

    public static void ConfigureServices(IServiceCollection services)
    {
        services.AddTestEnvironment();

        services.AddTransient<IDataRepository, MirrorRepository>();
        services.Configure<CommonBaseOptions>(options => options.TokenizerIsEnable = true);
        if (_configuration != null) services.Configure<DatabaseOptions>(_configuration.GetSection(nameof(DatabaseOptions)));

        services.AddSingleton<ILogger, TestLogger<SimpleStartup>>();
        services.AddTransient<ITokenizerProcessor, TokenizerProcessor>();
        services.AddTransient<ITokenizerService, TokenizerService>();
        services.AddHostedService<TokenizerActivatorService>();
    }

    public static void Configure(IApplicationBuilder app, ILoggerFactory loggerFactory)
    {
        app.UseRouting();
        app.UseEndpoints(ep => ep.MapControllers());
    }
}
