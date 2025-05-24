using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SearchEngine.Api.Services;
using SearchEngine.Api.Startup;
using SearchEngine.Data.Configuration;
using SearchEngine.Data.Contracts;
using SearchEngine.Infrastructure.Repository;
using SearchEngine.Service.Configuration;
using SearchEngine.Service.Contracts;
using SearchEngine.Service.Tokenizer;
using SearchEngine.Tests.Integrations.Extensions;
using SearchEngine.Tests.Units.Mocks;

namespace SearchEngine.Tests.Integrations.Api;

/// <summary>
/// Используется SQLite, информация по данной бд: https://www.sqlite.org/lang.html
/// Конфигурация регистрирует <b>MirrorRepository</b>
/// </summary>
internal class SqliteStartup(IConfiguration configuration)
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSqliteTestEnvironment();

        services.AddScoped<IDataRepository, MirrorRepository>();
        services.Configure<CommonBaseOptions>(options => options.TokenizerIsEnable = true);
        services.Configure<DatabaseOptions>(configuration.GetSection(nameof(DatabaseOptions)));

        services.AddSingleton<ILogger, NoopLogger<SqliteStartup>>();

        services.AddSingleton<ITokenizerProcessorFactory, TokenizerProcessorFactory>();

        services.AddSingleton<ITokenizerService, TokenizerService>();
        services.AddHostedService<ActivatorService>();
        services.AddSingleton<MigratorState>();

        services.AddDomainLayerDependencies();
    }

    public void Configure(IApplicationBuilder app, ILoggerFactory loggerFactory)
    {
        app.UseRouting();
        app.UseEndpoints(ep => ep.MapControllers());
    }
}

