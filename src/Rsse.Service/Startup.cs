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
using Microsoft.AspNetCore.DataProtection;// не удалять, трассировка
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.StaticFiles;
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
using Serilog;

namespace SearchEngine;

public class Startup(IConfiguration configuration, IWebHostEnvironment env)
{
    internal const string DefaultConnectionKey = "DefaultConnection";
    internal const string AdditionalConnectionKey = "AdditionalConnection";

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
        });

        services.Configure<CommonBaseOptions>(configuration.GetSection(nameof(CommonBaseOptions)));
        services.Configure<DatabaseOptions>(configuration.GetSection(nameof(DatabaseOptions)));


        var mysqlConnectionString = GetDefaultConnectionString();
        var npgsqlConnectionString = GetAdditionalConnectionString();
        if (string.IsNullOrEmpty(mysqlConnectionString) || string.IsNullOrEmpty(npgsqlConnectionString))
        {
            throw new NullReferenceException("Invalid connection string");
        }

        services.AddDbContext<MysqlCatalogContext>(options => options.UseMySql(mysqlConnectionString, _mySqlVersion));
        services.AddDbContext<NpgsqlCatalogContext>(options => options.UseNpgsql(npgsqlConnectionString));
        services.AddSingleton<IDbMigrator, MySqlDbMigrator>();
        services.AddSingleton<IDbMigrator, NpgsqlDbMigrator>();

        services.AddScoped<CatalogRepository<MysqlCatalogContext>>();
        services.AddScoped<CatalogRepository<NpgsqlCatalogContext>>();
        services.AddScoped<IDataRepository, MirrorRepository>();

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
            // полный вывод деталей ошибки и всех хедеров также доступен в сваггере
            app.UseDeveloperExceptionPage();
        }
        else
        {
            // app.UseExceptionHandler("/error");
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

    private string? GetDefaultConnectionString() => configuration.GetConnectionString(DefaultConnectionKey);
    private string? GetAdditionalConnectionString() => configuration.GetConnectionString(AdditionalConnectionKey);

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
        logger.LogInformation("Default connection string: {ConnectionString}", GetDefaultConnectionString());
        logger.LogInformation("Additional connection string: {ConnectionString}", GetAdditionalConnectionString());
        logger.LogInformation("Server GC: {IsServer}", GCSettings.IsServerGC);
        logger.LogInformation("CPU: {Cpus}", Environment.ProcessorCount);
    }
}
