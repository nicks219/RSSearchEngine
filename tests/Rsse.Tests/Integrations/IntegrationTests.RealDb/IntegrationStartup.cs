using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SearchEngine.Api.Authorization;
using SearchEngine.Api.Middleware;
using SearchEngine.Api.Services;
using SearchEngine.Api.Startup;
using SearchEngine.Data.Configuration;
using SearchEngine.Data.Contracts;
using SearchEngine.Infrastructure.Repository;
using SearchEngine.Service.Configuration;
using SearchEngine.Service.Contracts;
using SearchEngine.Service.Tokenizer;
using SearchEngine.Tests.Units.Infra;

namespace SearchEngine.Tests.Integrations.IntegrationTests.RealDb;

/// <summary>
/// Используются провайдеры до mysql и postgres.
/// Конфигурация регистрирует <b>MirrorRepository</b>
/// </summary>
public class IntegrationStartup(IConfiguration configuration)
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddDbsIntegrationTestEnvironment();

        services.AddScoped<IDataRepository, MirrorRepository>();
        services.Configure<CommonBaseOptions>(options =>
        {
            options.TokenizerIsEnable = true;
            options.CreateBackupForNewSong = true;
        });
        services.Configure<DatabaseOptions>(configuration.GetSection(nameof(DatabaseOptions)));

        services.AddSingleton<ILogger, NoopLogger<IntegrationStartup>>();

        services.AddSingleton<ITokenizerProcessorFactory, TokenizerProcessorFactory>();

        services.AddSingleton<ITokenizerService, TokenizerService>();
        services.AddHostedService<ActivatorService>();
        services.AddToolingDependencies();

        services
            .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(options =>
            {
                options.LoginPath = new PathString("/account/login");
                options.LogoutPath = new PathString("/account/logout");
                options.AccessDeniedPath = new PathString("/account/accessDenied");
                options.ReturnUrlParameter = "returnUrl";
                // todo уточнить коды ответа челенджа
                options.Events = new CookieAuthenticationEvents
                {
                    OnRedirectToLogin = context =>
                    {
                        context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                        context.Response.Headers["Shift"] = "301 Cancelled";
                        return Task.CompletedTask;
                    }
                };
            });

        // FullAccessPolicyName проверятся в контроллере migration/restore
        services
            .AddAuthorizationBuilder()
            .AddPolicy(Constants.FullAccessPolicyName, builder =>
            {
                builder.AddRequirements(new FullAccessRequirement());
            });
        services.AddSingleton<IAuthorizationHandler, FullAccessRequirementsHandler>();

        services.AddDomainLayerDependencies();
    }

    public void Configure(IApplicationBuilder app, ILoggerFactory loggerFactory)
    {
        app.UseRouting();
        app.UseMiddleware<ExceptionHandlingMiddleware>();

        app.UseAuthentication();

        app.UseAuthorization();

        app.UseEndpoints(ep => ep.MapControllers());
    }
}
