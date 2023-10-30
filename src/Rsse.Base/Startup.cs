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
using SearchEngine.Infrastructure.Engine;
using SearchEngine.Infrastructure.Engine.Contracts;
using SearchEngine.Infrastructure.Logger;
using SearchEngine.Infrastructure.Tokenizer;
using SearchEngine.Infrastructure.Tokenizer.Contracts;

namespace SearchEngine;

// TagIt: v1
// фронт после билда копируем руками из FrontRepository/ClientApp/build в Rsse.Base/ClientApp/build
// [TODO] поправь нейминг, это не RSSE

// TODO миграции
// export DOTNET_ROLL_FORWARD=LatestMajor
// Microsoft.EntityFrameworkCore.Design
// dotnet new tool-manifest
// dotnet tool update dotnet-ef (7.0.1)
// dotnet ef dbcontext list
// dotnet ef migrations list
// из папки RsseBase: dotnet ef migrations add Init -s "./" -p "../Rsse.Data"
// зафигачило миграцию в Rsse.Data

// TODO - удали каменты, отрефактори и сделай новую верстку:
// нейминг методов, async-await в js, роутер вместо моего "меню".
// по-хорошему переделать схему бд или хотя бы DTO.

public class Startup
{
    private const string MsSqlServer = "mssql";
    private const string MySqlServer = "mysql";
    private const string DefaultConnectionKey = "DefaultConnection";
    private const string DataSourceKey = "Data Source";

    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _env;

    public Startup(IConfiguration configuration, IWebHostEnvironment env)
    {
        _configuration = configuration;

        _env = env;
    }

    public void ConfigureServices(IServiceCollection services)
    {
        // if (_env.IsDevelopment()){}
        services.AddCors();

        services.AddHostedService<TokenizerActivatorService>();

        services.AddSingleton<ITokenizerService, TokenizerService>();

        services.AddTransient<ITokenizerProcessor, TokenizerProcessor>();

        services.AddSingleton<IDbBackup, MySqlDbBackup>();

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

        var sqlServerType = connectionString.Contains(DataSourceKey) ? MsSqlServer : MySqlServer;

        Action<DbContextOptionsBuilder> dbOptions = sqlServerType switch
        {
            MySqlServer => options => options.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 31))),
            // b=> b.MigrationsAssembly("Rsse.Data")
            // _ => options => options.UseSqlServer(connectionString),
            _ => throw new NotImplementedException("[unsupported db]")
        };

        services.AddDbContext<RsseContext>(dbOptions);

        services.AddScoped<IDataRepository, DataRepository>();

        services.AddMemoryCache(); // это нужно?

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
            app.UseExceptionHandler("/Error");
        }

        app.UseDefaultFiles();

        app.UseStaticFiles();

        // app.UseHttpsRedirection();

        app.UseRouting();

        // if (_env.IsDevelopment())
        {
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
        }

        app.UseAuthentication();

        app.UseAuthorization();

        app.UseEndpoints(endpoints => { endpoints.MapControllers(); });

        // логируем техническую информацию на старте:
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

    private string? GetConnectionString() => _configuration.GetConnectionString(DefaultConnectionKey);
}
