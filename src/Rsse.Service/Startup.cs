using System;
using System.Globalization;
using System.IO;
using System.Net;
using System.Runtime;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SearchEngine.Common.Auth;
using SearchEngine.Common.Configuration;
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
    private const string MsSql = "mssql";
    private const string MySql = "mysql";
    private const string DefaultConnectionKey = "DefaultConnection";
    private const string MsDataSourceKey = "Data Source";
    private const string DevelopmentCorsPolicy = nameof(DevelopmentCorsPolicy);
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

        var dbType = connectionString.Contains(MsDataSourceKey) ? MsSql : MySql;

        Action<DbContextOptionsBuilder> dbOptions = dbType switch
        {
            MySql => options => options.UseMySql(connectionString, _mySqlVersion),
            _ => throw new NotImplementedException("[unsupported db]")
        };

        services.AddDbContext<CatalogContext>(dbOptions);

        services.AddScoped<IDataRepository, CatalogRepository>();

        services.AddControllers();

        services
            .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(options =>
            {
                options.LoginPath = new PathString("/account/login");
                options.LogoutPath = new PathString("/account/logout");
                options.AccessDeniedPath = new PathString("/error/403");
                options.ReturnUrlParameter = "returnUrl";
                // todo проверить актуальность челенджа
                options.Events = new CookieAuthenticationEvents
                {
                    OnRedirectToLogin = context =>
                    {
                        context.Response.StatusCode = (int)HttpStatusCode.OK;
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

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env1, ILoggerFactory loggerFactory)
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
            // ручка error не реализована:
            app.UseExceptionHandler("/error");
        }

        app.UseDefaultFiles();

        app.UseStaticFiles();

        app.UseRouting();

        app.UseCors(DevelopmentCorsPolicy);

        app.UseAuthentication();

        app.UseAuthorization();
        // todo разбирайся с челенджем:
        app.Map("/error/403", conf => conf.Run(ctx =>
        {
            ctx.Response.StatusCode = 403;
            return Task.CompletedTask;
        }));

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });

        AddLogging(loggerFactory);
        LogSystemInfo(loggerFactory, isDevelopment, isProduction);
    }

    private string? GetConnectionString() => configuration.GetConnectionString(DefaultConnectionKey);

    private static void AddLogging(ILoggerFactory loggerFactory)
    {
        loggerFactory.AddFile(Path.Combine(Directory.GetCurrentDirectory(), LogFileName));
    }

    private void LogSystemInfo(ILoggerFactory loggerFactory, bool isDevelopment, bool isProduction)
    {
        var logger = loggerFactory.CreateLogger(typeof(FileLogger));

        logger.LogInformation("Application started at {Date}", DateTime.Now.ToString(CultureInfo.InvariantCulture));
        logger.LogInformation("Is 64-bit process: {Process}", Environment.Is64BitProcess.ToString());
        logger.LogInformation("Development: {IsDev}", isDevelopment);
        logger.LogInformation("Production: {IsProd}", isProduction);
        logger.LogInformation("Connection string: {ConnectionString}", GetConnectionString());
        logger.LogInformation("Server GC: {IsServer}", GCSettings.IsServerGC);
        logger.LogInformation("CPU: {Cpus}", Environment.ProcessorCount);
    }
}
