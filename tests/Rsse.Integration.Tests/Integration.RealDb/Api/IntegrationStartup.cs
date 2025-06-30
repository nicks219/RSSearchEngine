using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Rsse.Api.Authorization;
using Rsse.Api.Configuration;
using Rsse.Api.Middleware;
using Rsse.Api.Services;
using Rsse.Api.Startup;
using Rsse.Domain.Data.Configuration;
using Rsse.Domain.Data.Contracts;
using Rsse.Domain.Service.Configuration;
using Rsse.Domain.Service.Contracts;
using Rsse.Infrastructure.Repository;

namespace Rsse.Tests.Integration.RealDb.Api;

/// <summary>
/// Используются провайдеры до mysql и postgres.
/// Конфигурация регистрирует <b>MirrorRepository</b>
/// </summary>
public class IntegrationStartup(IConfiguration configuration)
{
    public void ConfigureServices(IServiceCollection services)
    {
        // перенесено в IntegrationWebAppFactory
        // services.AddDbsIntegrationTestEnvironment();

        services.AddScoped<IDataRepository, MirrorRepository>();
        services.Configure<CommonBaseOptions>(options =>
        {
            options.TokenizerIsEnable = true;
            options.CreateBackupForNewSong = true;
        });
        services.Configure<DatabaseOptions>(configuration.GetSection(nameof(DatabaseOptions)));

        //services.AddSingleton<ILogger, NoopLogger<IntegrationStartup>>();

        services.AddSingleton<ITokenizerApiClient, TokenizerApiClient>();
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
