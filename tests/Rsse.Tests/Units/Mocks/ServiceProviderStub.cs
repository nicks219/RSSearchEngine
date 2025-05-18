using System;
using Microsoft.Extensions.DependencyInjection;
using SearchEngine.Api.Startup;
using SearchEngine.Domain.Configuration;
using SearchEngine.Domain.Contracts;
using SearchEngine.Domain.Tokenizer;
using SearchEngine.Tests.Units.Mocks.Repo;

namespace SearchEngine.Tests.Units.Mocks;

/// <summary/> Для тестов, с двумя логгерами.
public class ServiceProviderStub
{
    internal readonly IServiceScope Scope;
    internal readonly IServiceProvider Provider;

    public ServiceProviderStub()
    {
        var services = new ServiceCollection();

        services.AddSingleton<IDataRepository, FakeCatalogRepository>();// один набор данных для группы тестов
        services.Configure<CommonBaseOptions>(options => options.TokenizerIsEnable = true);

        services.AddTransient<ITokenizerProcessor, TokenizerProcessor>();
        services.AddSingleton<ITokenizerService, TokenizerService>();

        // для тестов create
        services.AddDomainLayerDependencies();
        services.AddNoopDomainLayerLoggers();

        var serviceProvider = services.BuildServiceProvider();
        Provider = serviceProvider;
        Scope = serviceProvider.CreateScope();
    }
}
