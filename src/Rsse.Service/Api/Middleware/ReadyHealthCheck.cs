using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Rsse.Domain.Service.Contracts;

namespace Rsse.Api.Middleware;

/// <summary>
/// Проверка готовности сервиса принимать трафик.
/// </summary>
public class ReadyHealthCheck(ITokenizerApiClient tokenizer) : IHealthCheck
{
    /// <summary>
    /// Проверить доступность сервиса. Считаем доступным если инициализация токенизатора прошла успешно.
    /// </summary>
    /// <returns></returns>
    // todo: продумать fallback стратегию при неуспешной инициализации.
    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken _)
    {
        var isInitialized = tokenizer.IsInitialized();
        var healthCheckResult = isInitialized
            ? Task.FromResult(HealthCheckResult.Healthy("Ready"))
            : Task.FromResult(HealthCheckResult.Unhealthy("Not ready"));
        return healthCheckResult;
    }
}
