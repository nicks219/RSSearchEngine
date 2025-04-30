using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SearchEngine.Common.Configuration;
using SearchEngine.Data.Repository.Contracts;
using SearchEngine.Engine.Contracts;
using SearchEngine.Engine.Tokenizer;
using SearchEngine.Managers;
using SearchEngine.Tests.Units.Mocks.DatabaseRepo;

namespace SearchEngine.Tests.Units.Mocks;

/// <summary/> Для тестов
public class ServicesStubStartup<TService> where TService : class
{
    internal readonly IServiceScope Scope;
    internal readonly IServiceProvider Provider;

    public ServicesStubStartup()
    {
        var services = new ServiceCollection();

        services.AddSingleton<IDataRepository, FakeCatalogRepository>();// один набор данных для группы тестов
        services.Configure<CommonBaseOptions>(options => options.TokenizerIsEnable = true);

        services.AddSingleton<ILogger<TService>, NoopLogger<TService>>();
        services.AddTransient<ITokenizerProcessor, TokenizerProcessor>();
        services.AddSingleton<ITokenizerService, TokenizerService>();

        var serviceProvider = services.BuildServiceProvider();
        Provider = serviceProvider;
        Scope = serviceProvider.CreateScope();
    }
}
