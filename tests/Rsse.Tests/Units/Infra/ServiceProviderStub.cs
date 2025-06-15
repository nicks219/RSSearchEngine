using System;
using Microsoft.Extensions.DependencyInjection;
using SearchEngine.Api.Services;
using SearchEngine.Api.Startup;
using SearchEngine.Data.Contracts;
using SearchEngine.Service.Configuration;
using SearchEngine.Service.Contracts;
using SearchEngine.Service.Tokenizer;
using SearchEngine.Service.Tokenizer.Contracts;
using SearchEngine.Service.Tokenizer.SearchProcessor;

namespace SearchEngine.Tests.Units.Infra;

/// <summary/> Для тестов, с двумя логгерами.
public sealed class ServiceProviderStub : IDisposable
{
    internal readonly IServiceScope Scope;
    internal readonly IServiceProvider Provider;

    public ServiceProviderStub(SearchType searchType = SearchType.Original)
    {
        var services = new ServiceCollection();

        services.AddSingleton<IDataRepository, FakeCatalogRepository>();// один набор данных для группы тестов
        services.Configure<CommonBaseOptions>(options =>
        {
            options.TokenizerIsEnable = true;
            options.SearchType = searchType;
        });

        services.AddSingleton<ITokenizerService, TokenizerService>();

        services.AddSingleton<ITokenizerProcessorFactory, TokenizerProcessorFactory>();

        // для тестов create
        services.AddDomainLayerDependencies();
        services.AddNoopDomainLayerLoggers();

        var serviceProvider = services.BuildServiceProvider();
        Provider = serviceProvider;
        Scope = serviceProvider.CreateScope();
    }

    public void Dispose()
    {
        Scope.Dispose();
    }
}
