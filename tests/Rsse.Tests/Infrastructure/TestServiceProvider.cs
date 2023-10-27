using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SearchEngine.Configuration;
using SearchEngine.Data;
using SearchEngine.Data.Repository;
using SearchEngine.Data.Repository.Contracts;
using SearchEngine.Infrastructure.Cache;
using SearchEngine.Infrastructure.Cache.Contracts;
using SearchEngine.Infrastructure.Engine;
using SearchEngine.Infrastructure.Engine.Contracts;
using SearchEngine.Tests.Infrastructure.DAL;

namespace SearchEngine.Tests.Infrastructure;

public class TestServiceProvider<T> where T : class
{
    internal readonly IServiceScope ServiceScope;
    internal readonly IServiceProvider ServiceProvider;

    // Connection String для MsSql: private readonly string _connectionString = "Data Source=DESKTOP-I5CODE\\SSDSQL;Initial Catalog=rsse;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";

    private const string ConnectionString = @"Server=localhost;Database=tagit;Uid=1;Pwd=1;";

    public TestServiceProvider(bool useStubDataRepository = false)
    {
        var services = new ServiceCollection();

        if (!useStubDataRepository)
        {
            services.AddTransient<IDataRepository, DataRepository>();

            // MySql:
            services.AddDbContext<RsseContext>(options =>
                options.UseMySql(ConnectionString, new MySqlServerVersion(new Version(8, 0, 26))));
        }
        else
        {
            // выбор repo: стаб или настоящий - похоже, InMemory работает, но надо инициализировать базу:
            // services.AddTransient<IDataRepository, DataRepository>();// < из варианта выше.

            services.AddSingleton<IDataRepository, TestDataRepository>();// вариант для прогона тестов
            services.Configure<CommonBaseOptions>(o => o.TokenizerIsEnable = true);
        }

        services.AddSingleton<ILogger<T>, TestLogger<T>>();

        services.AddTransient<ITextProcessor, TextProcessor>();

        services.AddTransient<ICacheRepository, CacheRepository>();

        // services.AddDbContext<RsseContext>(options => options.UseSqlServer(_connectionString));
        services.AddDbContext<RsseContext>(options => options.UseInMemoryDatabase(databaseName: "rsse"));

        var serviceProvider = services.BuildServiceProvider();

        ServiceProvider = serviceProvider;

        ServiceScope = serviceProvider.CreateScope();
    }
}
