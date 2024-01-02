using System;
using System.Globalization;
using System.IO;
using System.Runtime;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SearchEngine.Common.Configuration;
using SearchEngine.Common.Logger;
using SearchEngine.Data.Context;
using SearchEngine.Data.Repository;
using SearchEngine.Data.Repository.Contracts;
using SearchEngine.Engine.Contracts;
using SearchEngine.Engine.Tokenizer;
using SearchEngine.Tools.MigrationAssistant;

namespace SearchEngine;

public class Startup
{
    private const string MsSql = "mssql";
    private const string MySql = "mysql";
    private const string DefaultConnectionKey = "DefaultConnection";
    private const string MsDataSourceKey = "Data Source";
    private const string DevelopmentCorsPolicy = nameof(DevelopmentCorsPolicy);
    private const string LogFileName = "service.log";

    private readonly ServerVersion _mySqlVersion = new MySqlServerVersion(new Version(8, 0, 31));
    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _env;

    private readonly string[] _allowedOrigins = {
        // dev сервер для JS:
        "https://localhost:5173",
        "http://localhost:5173",
        "https://127.0.0.1:5173",
        "http://127.0.0.1:5173",
        // same-origin на проде:
        "http://188.120.235.243:5000"
    };

    public Startup(IConfiguration configuration, IWebHostEnvironment env)
    {
        _configuration = configuration;

        _env = env;
    }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddHostedService<TokenizerActivatorService>();

        services.AddSingleton<ITokenizerService, TokenizerService>();

        services.AddTransient<ITokenizerProcessor, TokenizerProcessor>();

        services.AddSingleton<IDbMigrator, MySqlDbMigrator>();

        services.AddHttpContextAccessor();

        services.AddSwaggerGen(swaggerGenOptions =>
        {
            swaggerGenOptions.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "RSSearchEngine API", Version = "v5.1" });
        });

        services.Configure<CommonBaseOptions>(_configuration.GetSection(nameof(CommonBaseOptions)));

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
                options.LoginPath = new PathString("/Account/Login/");
            });

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

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
    {
        var isDevelopment = _env.IsDevelopment();
        var isProduction = _env.IsProduction();

        if (isDevelopment)
        {
            app.UseDeveloperExceptionPage();

            app.UseSwagger();

            app.UseSwaggerUI(c => { c.SwaggerEndpoint("/swagger/v1/swagger.json", "rsse v5.1"); });
        }
        else
        {
            // ручка error нигде не определена:
            app.UseExceptionHandler("/error");
        }

        app.UseDefaultFiles();

        app.UseStaticFiles();

        app.UseRouting();

        app.UseCors(DevelopmentCorsPolicy);

        app.UseAuthentication();

        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });

        UseLogging(loggerFactory);

        LogSystemInfo(loggerFactory, isDevelopment, isProduction);
    }

    private string? GetConnectionString() => _configuration.GetConnectionString(DefaultConnectionKey);

    private static void UseLogging(ILoggerFactory loggerFactory)
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
