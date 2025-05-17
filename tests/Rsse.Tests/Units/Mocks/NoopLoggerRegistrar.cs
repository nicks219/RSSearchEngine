using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SearchEngine.Domain.Managers;
using SearchEngine.Domain.Tokenizer;

namespace SearchEngine.Tests.Units.Mocks;

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
        services.AddSingleton<ILogger<DeleteManager>, NoopLogger<DeleteManager>>();
        services.AddSingleton<ILogger<AccountManager>, NoopLogger<AccountManager>>();
        services.AddSingleton<ILogger<CatalogManager>, NoopLogger<CatalogManager>>();
        services.AddSingleton<ILogger<ComplianceSearchManager>, NoopLogger<ComplianceSearchManager>>();
        services.AddSingleton<ILogger<CreateManager>, NoopLogger<CreateManager>>();
        services.AddSingleton<ILogger<ReadManager>, NoopLogger<ReadManager>>();
        services.AddSingleton<ILogger<UpdateManager>, NoopLogger<UpdateManager>>();

        services.AddSingleton<ILogger<TokenizerService>, NoopLogger<TokenizerService>>();
    }
}
