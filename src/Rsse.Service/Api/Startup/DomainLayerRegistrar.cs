using Microsoft.Extensions.DependencyInjection;
using SearchEngine.Domain.Services;

namespace SearchEngine.Api.Startup;

/// <summary>
/// Расширение, регистрирующее зависимости для слоя бизнес-логики.
/// </summary>
public static class DomainLayerRegistrar
{
    /// <summary>
    /// Добавить зависимости бизнес-слоя в DI.
    /// </summary>
    public static void AddDomainLayerDependencies(this IServiceCollection services)
    {
        services.AddScoped<DeleteService>();
        services.AddScoped<AccountService>();
        services.AddScoped<CatalogService>();
        services.AddScoped<ComplianceSearchService>();
        services.AddScoped<CreateService>();
        services.AddScoped<ReadService>();
        services.AddScoped<UpdateService>();
    }
}
