using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SearchEngine.Domain.Configuration;
using SearchEngine.Domain.Contracts;
using SearchEngine.Domain.Tokenizer;
using SearchEngine.Tests.Units.Mocks.Repo;

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
