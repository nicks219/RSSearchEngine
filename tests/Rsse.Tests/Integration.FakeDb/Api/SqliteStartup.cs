using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Rsse.Api.Configuration;
using Rsse.Api.Middleware;
using Rsse.Api.Services;
using Rsse.Api.Startup;
using Rsse.Domain.Data.Configuration;
using Rsse.Domain.Data.Contracts;
using Rsse.Domain.Service.Contracts;
using Rsse.Infrastructure.Repository;
using Rsse.Tests.Integration.FakeDb.Extensions;
using Rsse.Tests.Units.Infra;
using RsseEngine.SearchType;

namespace Rsse.Tests.Integration.FakeDb.Api;

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
        services.Configure<CommonBaseOptions>(options =>
        {
            options.TokenizerIsEnable = true;
            options.ExtendedSearchType = ExtendedSearchType.Legacy;
            options.ReducedSearchType = ReducedSearchType.Legacy;
        });
        services.Configure<DatabaseOptions>(configuration.GetSection(nameof(DatabaseOptions)));

        services.AddSingleton<ILogger, NoopLogger<SqliteStartup>>();

        services.AddSingleton<ITokenizerApiClient, TokenizerApiClient>();
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

