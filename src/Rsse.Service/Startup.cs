using System;
using System.Globalization;
using System.IO;
using System.Net;
using System.Runtime;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SearchEngine.Common.Auth;
using SearchEngine.Common.Configuration;
using SearchEngine.Common.Extensions;
using SearchEngine.Common.Logger;
using SearchEngine.Data.Context;
using SearchEngine.Data.Repository;
using SearchEngine.Data.Repository.Contracts;
using SearchEngine.Engine.Contracts;
using SearchEngine.Engine.Tokenizer;
using SearchEngine.Tools.MigrationAssistant;

namespace SearchEngine;

public class Startup(IConfiguration configuration, IWebHostEnvironment env)
{
    private const string DefaultConnectionKey = "DefaultConnection";
    private const string LogFileName = "service.log";

    private readonly ServerVersion _mySqlVersion = new MySqlServerVersion(new Version(8, 0, 31));

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
        services.AddHostedService<TokenizerActivatorService>();

        services.AddSingleton<ITokenizerService, TokenizerService>();

        services.AddTransient<ITokenizerProcessor, TokenizerProcessor>();

        services.AddSingleton<IDbMigrator, MySqlDbMigrator>();

        services.AddHttpContextAccessor();

        services.AddSwaggerGen(swaggerGenOptions =>
        {
            swaggerGenOptions.SwaggerDoc(Constants.SwaggerDocNameSegment, new Microsoft.OpenApi.Models.OpenApiInfo
            {
                Title = Constants.SwaggerTitle,
                Version = Constants.ApiVersion
            });
        });

        services.Configure<CommonBaseOptions>(configuration.GetSection(nameof(CommonBaseOptions)));

        var connectionString = GetConnectionString();
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new NullReferenceException("Invalid connection string");
        }

        services.AddDbContext<CatalogContext>(options => options.UseMySql(connectionString, _mySqlVersion));

        services.AddScoped<IDataRepository, CatalogRepository>();

        services.AddControllers();

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
                policyBuilder.WithMethods("GET", "POST", "DELETE", "OPTIONS");
            });
        });

        services.AddMetricsInternal();
        services.AddRateLimiterInternal();
#if TRACING_ENABLE
        services.AddTracingInternal();
        services.AddDataProtection()
            .PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(AppContext.BaseDirectory, "rsse-keys")))
            .SetApplicationName("rsse-app");
#endif
    }

    public void Configure(IApplicationBuilder app, ILoggerFactory loggerFactory)
    {
        var isDevelopment = env.IsDevelopment();
        var isProduction = env.IsProduction();

        if (isDevelopment)
        {
            app.UseDeveloperExceptionPage();

            app.UseSwagger();

            app.UseSwaggerUI(uiOptions =>
            {
                uiOptions.SwaggerEndpoint($"/swagger/{Constants.SwaggerDocNameSegment}/swagger.json", Constants.ApplicationFullName);
            });
        }
        else
        {
            // app.UseExceptionHandler("/error");
        }

        app.UseDefaultFiles();

        app.UseStaticFiles();

        app.UseRouting();

        app.UseCors(Constants.DevelopmentCorsPolicy);

        app.UseAuthentication();

        app.UseAuthorization();

        app.UseRateLimiter();
        app.UseEndpoints(endpoints =>
        {
            endpoints.Map("/account/accessDenied", async next =>
            {
                next.Response.StatusCode = 403;
                next.Response.ContentType = "text/plain";
                await next.Response.WriteAsync($"{next.Request.Method}: access denied.");
            }).RequireAuthorization();
            endpoints.MapControllers();
            endpoints.MapPrometheusScrapingEndpoint().RequireRateLimiting(Constants.MetricsHandlerPolicy);
        });

        AddLogging(loggerFactory);
        LogSystemInfo(loggerFactory, isDevelopment, isProduction);
    }

    private string? GetConnectionString() => configuration.GetConnectionString(DefaultConnectionKey);

    private static void AddLogging(ILoggerFactory loggerFactory)
    {
        loggerFactory.AddFileInternal(Path.Combine(Directory.GetCurrentDirectory(), LogFileName));
    }

    private void LogSystemInfo(ILoggerFactory loggerFactory, bool isDevelopment, bool isProduction)
    {
        var logger = loggerFactory.CreateLogger(typeof(FileLoggerInternal));

        logger.LogInformation("Application started at {Date}", DateTime.Now.ToString(CultureInfo.InvariantCulture));
        logger.LogInformation("Is 64-bit process: {Process}", Environment.Is64BitProcess.ToString());
        logger.LogInformation("Development: {IsDev}", isDevelopment);
        logger.LogInformation("Production: {IsProd}", isProduction);
        logger.LogInformation("Connection string: {ConnectionString}", GetConnectionString());
        logger.LogInformation("Server GC: {IsServer}", GCSettings.IsServerGC);
        logger.LogInformation("CPU: {Cpus}", Environment.ProcessorCount);
    }
}
