using Microsoft.Extensions.DependencyInjection;
using SearchEngine.Domain.Managers;

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
        services.AddScoped<DeleteManager>();
        services.AddScoped<AccountManager>();
        services.AddScoped<CatalogManager>();
        services.AddScoped<ComplianceSearchManager>();
        services.AddScoped<CreateManager>();
        services.AddScoped<ReadManager>();
        services.AddScoped<UpdateManager>();
    }
}
