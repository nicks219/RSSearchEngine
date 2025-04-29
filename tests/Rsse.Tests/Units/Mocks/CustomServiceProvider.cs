using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SearchEngine.Common.Configuration;
using SearchEngine.Data.Repository.Contracts;
using SearchEngine.Engine.Contracts;
using SearchEngine.Engine.Tokenizer;
using SearchEngine.Tests.Units.Mocks.DatabaseRepo;

namespace SearchEngine.Tests.Units.Mocks;

/// <summary/> Для тестов
public class CustomServiceProvider<TScope> where TScope : class
{
    internal readonly IServiceScope Scope;
    internal readonly IServiceProvider Provider;

    public CustomServiceProvider()
    {
        var services = new ServiceCollection();

        services.AddSingleton<IDataRepository, FakeCatalogRepository>();
        services.Configure<CommonBaseOptions>(options => options.TokenizerIsEnable = true);

        services.AddSingleton<ILogger<TScope>, NoopLogger<TScope>>();
        services.AddTransient<ITokenizerProcessor, TokenizerProcessor>();
        services.AddTransient<ITokenizerService, TokenizerService>();

        var serviceProvider = services.BuildServiceProvider();
        Provider = serviceProvider;
        Scope = serviceProvider.CreateScope();
    }
}
