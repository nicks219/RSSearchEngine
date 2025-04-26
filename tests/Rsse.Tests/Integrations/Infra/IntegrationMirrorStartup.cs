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
/// Используются провайдеры до mysql и postgres.
/// Конфигурация регистрирует <b>MirrorRepository</b>
/// </summary>
public class IntegrationMirrorStartup
{
    private static IConfiguration? _configuration;

    public IntegrationMirrorStartup(IConfiguration configuration) => _configuration = configuration;

    public static void ConfigureServices(IServiceCollection services)
    {
        services.AddDbsTestEnvironment();

        services.AddTransient<IDataRepository, MirrorRepository>();
        services.Configure<CommonBaseOptions>(options => options.TokenizerIsEnable = true);
        if (_configuration != null) services.Configure<DatabaseOptions>(_configuration.GetSection(nameof(DatabaseOptions)));

        services.AddSingleton<ILogger, NoopLogger<IntegrationMirrorStartup>>();
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
