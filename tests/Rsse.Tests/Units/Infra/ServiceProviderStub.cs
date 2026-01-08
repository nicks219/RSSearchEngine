using System;
using Microsoft.Extensions.DependencyInjection;
using Rsse.Api.Configuration;
using Rsse.Api.Services;
using Rsse.Api.Startup;
using Rsse.Domain.Data.Contracts;
using Rsse.Domain.Service.Contracts;
using SimpleEngine.SearchType;

namespace Rsse.Tests.Units.Infra;

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
