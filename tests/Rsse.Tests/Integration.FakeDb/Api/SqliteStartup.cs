using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SearchEngine.Api.Middleware;
using SearchEngine.Api.Services;
using SearchEngine.Api.Startup;
using SearchEngine.Data.Configuration;
using SearchEngine.Data.Contracts;
using SearchEngine.Infrastructure.Repository;
using SearchEngine.Services.Configuration;
using SearchEngine.Services.Contracts;
using SearchEngine.Tests.Integration.FakeDb.Extensions;
using SearchEngine.Tests.Units.Infra;
using SearchEngine.Tokenizer;
using SearchEngine.Tokenizer.Contracts;

namespace SearchEngine.Tests.Integration.FakeDb.Api;

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
        services.AddScoped<DbDataProvider>();
    }

    public void Configure(IApplicationBuilder app, ILoggerFactory loggerFactory)
    {
        app.UseRouting();
        app.UseMiddleware<ExceptionHandlingMiddleware>();
        app.UseEndpoints(ep => ep.MapControllers());
    }
}

