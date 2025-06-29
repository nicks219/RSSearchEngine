using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Rsse.Api.Services;
using Rsse.Domain.Service.Api;
using CreateService = Rsse.Domain.Service.Api.CreateService;

namespace Rsse.Tests.Units.Infra;

/// <summary>
/// Регистратор логгеров для тестов.
/// </summary>
public static class NoopLoggerRegistrar
{
    /// <summary>
    /// Зарегистрировать тестовые логгеры.
    /// </summary>
    public static void AddNoopDomainLayerLoggers(this IServiceCollection services)
    {
        services.AddSingleton<ILogger<DeleteService>, NoopLogger<DeleteService>>();
        services.AddSingleton<ILogger<AccountService>, NoopLogger<AccountService>>();
        services.AddSingleton<ILogger<CatalogService>, NoopLogger<CatalogService>>();
        services.AddSingleton<ILogger<ComplianceSearchService>, NoopLogger<ComplianceSearchService>>();
        services.AddSingleton<ILogger<CreateService>, NoopLogger<CreateService>>();
        services.AddSingleton<ILogger<ReadService>, NoopLogger<ReadService>>();
        services.AddSingleton<ILogger<UpdateService>, NoopLogger<UpdateService>>();

        services.AddSingleton<ILogger<TokenizerApiClient>, NoopLogger<TokenizerApiClient>>();
    }
}
