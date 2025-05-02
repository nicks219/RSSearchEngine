using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
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
using SearchEngine.Api.Logger;
using SearchEngine.Domain.Configuration;
using SearchEngine.Domain.Contracts;
using SearchEngine.Domain.Tokenizer;
using SearchEngine.Infrastructure.Repository;
using SearchEngine.Tests.Integrations.Extensions;

namespace SearchEngine.Tests.Integrations.Api;

/// <summary>
/// Копия класса настроек сервиса с настроенной авторизацией.
/// </summary>
[Obsolete("заменён на Startup сервиса")]
[ExcludeFromCodeCoverage]
public class SqliteAccessControlStartup(IConfiguration configuration)
{
    private const string DevelopmentCorsPolicy = nameof(DevelopmentCorsPolicy);
    private const string LogFileName = "service.log";

    private readonly string[] _allowedOrigins = {
        // dev сервер для JS:
        "https://localhost:5173",
        "http://localhost:5173",
        "https://127.0.0.1:5173",
        "http://127.0.0.1:5173",
        // same-origin на проде:
        "http://188.120.235.243:5000"
    };

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSqliteTestEnvironment();

        services.AddScoped<IDataRepository, MirrorRepository>();

        services.AddSingleton<ITokenizerService, TokenizerService>();

        services.AddTransient<ITokenizerProcessor, TokenizerProcessor>();

        // служба также заполняет бд
        services.AddHostedService<ActivatorService>();

        services.AddHttpContextAccessor();

        services.Configure<CommonBaseOptions>(configuration.GetSection(nameof(CommonBaseOptions)));
        // в настройках выбор возможности операций на двух контекстах
        services.Configure<DatabaseOptions>(configuration.GetSection(nameof(DatabaseOptions)));

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

        services
            .AddAuthorizationBuilder()
            .AddPolicy(Constants.FullAccessPolicyName, builder =>
            {
                builder.AddRequirements(new FullAccessRequirement());
            });

        services.AddSingleton<IAuthorizationHandler, FullAccessRequirementsHandler>();
        services.AddCors(builder =>
        {
            builder.AddPolicy(DevelopmentCorsPolicy, policyBuilder =>
            {
                policyBuilder.WithOrigins(_allowedOrigins).AllowCredentials();
                policyBuilder.WithHeaders("Content-Type");
                policyBuilder.WithMethods("GET", "POST", "DELETE", "OPTIONS");
            });
        });
    }

    public void Configure(IApplicationBuilder app, ILoggerFactory loggerFactory)
    {
        app.UseDefaultFiles();

        app.UseRouting();

        app.UseCors(DevelopmentCorsPolicy);

        app.UseAuthentication();

        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            endpoints.Map("/account/accessDenied", async next =>
            {
                next.Response.StatusCode = 403;
                next.Response.ContentType = "text/plain";
                await next.Response.WriteAsync($"{next.Request.Method}: access denied.");
            }).RequireAuthorization();
            endpoints.MapControllers();
        });

        AddLoggingInternal(loggerFactory);
    }

    private static void AddLoggingInternal(ILoggerFactory loggerFactory)
    {
        loggerFactory.AddFileLoggerProviderInternal(Path.Combine(Directory.GetCurrentDirectory(), LogFileName));
    }
}
