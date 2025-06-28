using System;
using System.Globalization;
using System.IO;
using System.Net;
using System.Runtime;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Rsse.Api.Authorization;
using Rsse.Api.Configuration;
using Rsse.Api.Middleware;
using Rsse.Api.Services;
using Rsse.Domain.Data.Configuration;
using Rsse.Domain.Data.Contracts;
using Rsse.Domain.Service.Configuration;
using Rsse.Domain.Service.Contracts;
using Rsse.Infrastructure.Context;
using Rsse.Infrastructure.Repository;
using Serilog;

namespace Rsse.Api.Startup;

/// <summary>
/// Настройка зависимостей и пайплайна запроса сервиса.
/// </summary>
public class Startup(IConfiguration configuration, IWebHostEnvironment env)
{
    internal const string DefaultConnectionKey = "DefaultConnection";
    internal const string AdditionalConnectionKey = "AdditionalConnection";

    private readonly string[] _allowedOrigins =
    [
        // dev сервер для JS:
        "https://localhost:5173",
        "http://localhost:5173",
        "https://127.0.0.1:5173",
        "http://127.0.0.1:5173",
        // same-origin на проде:
        "http://188.120.235.243:5000"
    ];

    /// <summary>
    /// Настроить зависимости сервиса.
    /// </summary>
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddHostedService<ActivatorService>();

        services.AddSingleton<ITokenizerApiClient, TokenizerApiClient>();

        services.AddHttpContextAccessor();

        services.AddSwaggerGen(swaggerGenOptions =>
        {
            swaggerGenOptions.EnableAnnotations();
            swaggerGenOptions.SwaggerDoc(Constants.SwaggerDocNameSegment, new Microsoft.OpenApi.Models.OpenApiInfo
            {
                Title = Constants.SwaggerTitle,
                Version = Constants.ApiVersion
            });
        });

        services.Configure<Microsoft.AspNetCore.Mvc.JsonOptions>(options =>
        {
            options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter<DatabaseType>());
            options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter<ElectionType>());
        });

        services.Configure<ElectionTypeOptions>(o => o.ElectionType = ElectionType.SqlRandom);
        services.Configure<CommonBaseOptions>(configuration.GetSection(nameof(CommonBaseOptions)));
        services.Configure<DatabaseOptions>(configuration.GetSection(nameof(DatabaseOptions)));

        services.TryAddCatalogStores(configuration, env);

        services.AddToolingDependencies();

        services.AddScoped<CatalogRepository<MysqlCatalogContext>>();
        services.AddScoped<CatalogRepository<NpgsqlCatalogContext>>();
        services.AddScoped<IDataRepository, MirrorRepository>();
        services.AddScoped<DbDataProvider>();

        services.AddControllers();
        services.AddDomainLayerDependencies();

        services
            .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(options =>
            {
                options.LoginPath = new PathString("/account/login");
                options.LogoutPath = new PathString("/account/logout");
                options.AccessDeniedPath = new PathString("/account/accessDenied");
                options.ReturnUrlParameter = "returnUrl";
                // todo: уточнить коды ответа челенджа
                options.Events = new CookieAuthenticationEvents
                {
                    OnRedirectToLogin = context =>
                    {
                        context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                        context.Response.Headers[Constants.ShiftHeaderName] = Constants.ShiftHeaderValue;
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
            builder.AddPolicy(Constants.DevelopmentCorsPolicy, policyBuilder =>
            {
                policyBuilder.WithOrigins(_allowedOrigins).AllowCredentials();
                policyBuilder.WithHeaders("Content-Type");
                policyBuilder.WithMethods("GET", "POST", "DELETE", "PUT", "OPTIONS");
            });
        });

        services.AddHealthChecks().AddCheck<ReadyHealthCheck>("ready.check", tags: ["ready"]);
        services.AddRateLimiterInternal();
        services
            .AddDataProtection()
            .PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(AppContext.BaseDirectory, "rsse-keys")))
            .SetApplicationName("rsse-app");

#if TRACING_ENABLE
        services.AddMetricsAndTracingInternal(configuration);
#endif
    }

    /// <summary>
    /// Настроить найплайн обработки запроса.
    /// </summary>
    public void Configure(IApplicationBuilder app)
    {
        var isDevelopment = env.IsDevelopment();
        var isProduction = env.IsProduction();

        if (isDevelopment)
        {
            // полный вывод деталей ошибки и всех хедеров также доступен в сваггере
            app.UseDeveloperExceptionPage();
        }

        app.UseSwagger();

        app.UseSwaggerUI(uiOptions =>
        {
            uiOptions.SwaggerEndpoint($"/swagger/{Constants.SwaggerDocNameSegment}/swagger.json",
                Constants.ApplicationFullName);
        });

        app.UseDefaultFiles();

        app.UseStaticFiles(new StaticFileOptions
        {
            OnPrepareResponse = ctx =>
            {
                // скрываем дампы от неавторизованного доступа
                if ((ctx.File.Name.StartsWith("dump") || ctx.File.Name.StartsWith("backup"))
                    && ctx.Context.User.Identity?.IsAuthenticated != true)
                {
                    ctx.Context.Abort();
                    Log.Information("abort unauthorized download request");
                }
            }
        });

        app.UseRouting();
        app.UseMiddleware<ExceptionHandlingMiddleware>();
        app.UseMiddleware<SetActivityStatusMiddleware>();

        app.UseCors(Constants.DevelopmentCorsPolicy);

        app.UseAuthentication();

        app.UseAuthorization();

        app.UseRateLimiter();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapHealthChecks("/system/live",
                new HealthCheckOptions
                {
                    Predicate = _ => false
                });
            endpoints.MapHealthChecks("/system/ready",
                new HealthCheckOptions
                {
                    Predicate = check => check.Tags.Contains("ready")
                });
            endpoints.Map("/account/accessDenied", async next =>
            {
                next.Response.StatusCode = 403;
                next.Response.ContentType = "text/plain";
                await next.Response.WriteAsync($"{next.Request.Method}: access denied.");
            }).RequireAuthorization();
            // endpoints.MapPrometheusScrapingEndpoint();
            endpoints.MapControllers();

#if TRACING_ENABLE
            // endpoints.MapPrometheusScrapingEndpoint().RequireRateLimiting(Constants.MetricsHandlerPolicy);
#endif
        });

        LogSystemInfo(isDevelopment, isProduction);
    }

    private string? GetDefaultConnectionString() => configuration.GetConnectionString(DefaultConnectionKey);
    private string? GetAdditionalConnectionString() => configuration.GetConnectionString(AdditionalConnectionKey);

    private void LogSystemInfo(bool isDevelopment, bool isProduction)
    {
        var podName = Environment.GetEnvironmentVariable("POD_NAME");

        Log.ForContext<Startup>().Information("Application started at {Date}", DateTime.Now.ToString(CultureInfo.InvariantCulture));
        Log.ForContext<Startup>().Information("Is 64-bit process: {Process}", Environment.Is64BitProcess.ToString());
        Log.ForContext<Startup>().Information("Development: {IsDev}", isDevelopment);
        Log.ForContext<Startup>().Information("Production: {IsProd}", isProduction);
        Log.ForContext<Startup>().Information("Default connection string: {ConnectionString}", GetDefaultConnectionString());
        Log.ForContext<Startup>().Information("Additional connection string: {ConnectionString}", GetAdditionalConnectionString());
        Log.ForContext<Startup>().Information("Server GC: {IsServer}", GCSettings.IsServerGC);
        Log.ForContext<Startup>().Information("CPU: {Cpus}", Environment.ProcessorCount);
        Log.ForContext<Startup>().Information("Pod name: '{PodName}'", podName);
    }
}
