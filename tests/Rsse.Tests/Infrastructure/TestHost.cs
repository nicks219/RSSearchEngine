using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RandomSongSearchEngine.Data;
using RandomSongSearchEngine.Data.Repository;
using RandomSongSearchEngine.Data.Repository.Contracts;
using RandomSongSearchEngine.Infrastructure.Cache;
using RandomSongSearchEngine.Infrastructure.Cache.Contracts;
using RandomSongSearchEngine.Infrastructure.Engine;
using RandomSongSearchEngine.Infrastructure.Engine.Contracts;

namespace RandomSongSearchEngine.Tests.Infrastructure;

public class TestHost<T> where T : class
{
    public readonly IServiceScope ServiceScope;
    
    public readonly IServiceProvider ServiceProvider;

    // Connection String для MsSql: private readonly string _connectionString = "Data Source=DESKTOP-I5CODE\\SSDSQL;Initial Catalog=rsse;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";

    private const string ConnectionString = @"Server=localhost;Database=rsse;Uid=1;Pwd=1;";

    public TestHost(bool stubRepository = false)
    {
        var services = new ServiceCollection();

        if (!stubRepository)
        {
            services.AddTransient<IDataRepository, DataRepository>();

            // MySql
            services.AddDbContext<RsseContext>(options =>
                options.UseMySql(ConnectionString, new MySqlServerVersion(new Version(8, 0, 26))));
        }
        else
        {
            services.AddTransient<IDataRepository, TestDataRepository>();
        }

        services.AddTransient<ILogger<T>, FakeLogger<T>>();

        services.AddTransient<ITextProcessor, TextProcessor>();

        services./*AddSingleton*/AddTransient<ICacheRepository, CacheRepository>();

        // MsSql
        // services.AddDbContext<RsseContext>(options => options.UseSqlServer(_connectionString));

        // вариант для тестов
        // services.AddDbContext<RsseContext>(options => options.UseInMemoryDatabase(databaseName: "rsse"));

        var serviceProvider = services.BuildServiceProvider();

        ServiceProvider = serviceProvider;

        ServiceScope = serviceProvider.CreateScope();
    }
    
    public IServiceScope CreateScope()
    {
        return ServiceProvider.CreateScope();
    }
}

public class CustomServiceScopeFactory : IServiceScopeFactory
{
    private readonly IServiceProvider _serviceProvider;

    public CustomServiceScopeFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }
    
    public IServiceScope CreateScope()
    {
        return _serviceProvider.CreateScope();
    }
}

public class FakeLogger<TReadModel> : ILogger<TReadModel>
{
    public IDisposable BeginScope<TState>(TState state)
    {
        throw new NotImplementedException();
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        throw new NotImplementedException();
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, 
        Exception? exception, Func<TState, Exception, string> formatter)
    {
        FakeLoggerErrors.ExceptionMessage = exception?.Message;
        
        FakeLoggerErrors.LogErrorMessage = state?.ToString();
    }
}

public static class FakeLoggerErrors
{
    public static string? ExceptionMessage { get; set; }
    public static string? LogErrorMessage { get; set; }
}