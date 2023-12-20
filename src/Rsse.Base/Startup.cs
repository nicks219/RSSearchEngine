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
using SearchEngine.Configuration;
using SearchEngine.Data;
using SearchEngine.Data.Repository;
using SearchEngine.Data.Repository.Contracts;
using SearchEngine.Infrastructure;
using SearchEngine.Infrastructure.Logger;
using SearchEngine.Infrastructure.Tokenizer;
using SearchEngine.Infrastructure.Tokenizer.Contracts;

namespace SearchEngine;

public class Startup
{
    private const string MsSql = "mssql";
    private const string MySql = "mysql";
    private const string DefaultConnectionKey = "DefaultConnection";
    private const string MsDataSourceKey = "Data Source";

    private readonly ServerVersion _mySqlVersion = new MySqlServerVersion(new Version(8, 0, 31));
    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _env;

    public Startup(IConfiguration configuration, IWebHostEnvironment env)
    {
        _configuration = configuration;

        _env = env;
    }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddCors();

        services.AddHostedService<TokenizerActivatorService>();

        services.AddSingleton<ITokenizerService, TokenizerService>();

        services.AddTransient<ITokenizerProcessor, TokenizerProcessor>();

        services.AddSingleton<IDbMigrator, MySqlDbMigrator>();

        services.AddHttpContextAccessor();

        services.AddSwaggerGen(swaggerGenOptions =>
        {
            swaggerGenOptions.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "Nick", Version = "v1" });
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
            // b => b.MigrationsAssembly("Rsse.Data")
            // _ => options => options.UseSqlServer(connectionString),
            _ => throw new NotImplementedException("[unsupported db]")
        };

        services.AddDbContext<RsseContext>(dbOptions);

        services.AddScoped<IDataRepository, DataRepository>();

        services.AddMemoryCache(); // это где-либо используется?

        services.AddControllers();

        services
            .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(options =>
            {
                options.LoginPath = new PathString("/Account/Login/");
            });
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
    {
        if (_env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();

            app.UseSwagger();

            app.UseSwaggerUI(c => { c.SwaggerEndpoint("/swagger/v1/swagger.json", "Nick V1"); });
        }
        else
        {
            // ручка error нигде не определена:
            app.UseExceptionHandler("/error");
        }

        app.UseDefaultFiles();

        app.UseStaticFiles();

        app.UseRouting();

        app.UseCors(builder =>
        {
            builder.WithOrigins(
                    "http://localhost:3000",
                    "http://127.0.0.1:3000",
                    // same-origin на проде
                    "http://188.120.235.243:5000")
                .AllowCredentials();
            builder.WithHeaders("Content-type");
            builder.WithMethods("GET", "POST", "DELETE", "OPTIONS");
        });

        app.UseAuthentication();

        app.UseAuthorization();

        app.UseEndpoints(endpoints => { endpoints.MapControllers(); });

        UseLogging(loggerFactory, env);
    }

    private string? GetConnectionString() => _configuration.GetConnectionString(DefaultConnectionKey);

    private void UseLogging(ILoggerFactory loggerFactory, IHostEnvironment env)
    {
        loggerFactory.AddFile(Path.Combine(Directory.GetCurrentDirectory(), "logger.txt"));
        var logger = loggerFactory.CreateLogger(typeof(FileLogger));

        logger.LogInformation(
            "Application started at {Date}, is 64-bit process: {Process}",
            DateTime.Now.ToString(CultureInfo.InvariantCulture),
            Environment.Is64BitProcess.ToString());

        logger.LogInformation("IsDevelopment: {IsDev}", env.IsDevelopment().ToString());
        logger.LogInformation("IsProduction: {IsProd}", env.IsProduction().ToString());
        logger.LogInformation("Connection string here: {ConnectionString}", GetConnectionString());
        logger.LogInformation("Server GC: {IsServer}", GCSettings.IsServerGC);
        logger.LogInformation("CPU: {Cpus}", Environment.ProcessorCount);
    }
}
