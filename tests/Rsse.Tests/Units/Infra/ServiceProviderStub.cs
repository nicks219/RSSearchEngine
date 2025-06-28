using System;
using Microsoft.Extensions.DependencyInjection;
using RsseEngine.SearchType;
using SearchEngine.Api.Configuration;
using SearchEngine.Api.Services;
using SearchEngine.Api.Startup;
using SearchEngine.Data.Contracts;
using SearchEngine.Service.Contracts;

namespace SearchEngine.Tests.Units.Infra;

/// <summary/> Для тестов, с двумя логгерами.
public sealed class ServiceProviderStub : IDisposable
{
    internal readonly IServiceScope Scope;
    internal readonly IServiceProvider Provider;

    public ServiceProviderStub(ExtendedSearchType extendedSearchType = ExtendedSearchType.Legacy,
        ReducedSearchType reducedSearchType = ReducedSearchType.Legacy)
    {
        var services = new ServiceCollection();

        services.AddSingleton<IDataRepository, FakeCatalogRepository>();// один набор данных для группы тестов
        services.Configure<CommonBaseOptions>(options =>
        {
            options.TokenizerIsEnable = true;
            options.ExtendedSearchType = extendedSearchType;
            options.ReducedSearchType = reducedSearchType;
        });

        services.AddSingleton<ITokenizerApiClient, TokenizerApiClient>();

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
