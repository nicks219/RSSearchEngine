#if TRACING_ENABLE
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace SearchEngine.Api.Startup;

/// <summary>
/// Расширение функционала поставки трассировок.
/// </summary>
internal static class TracingExtensions
{
    /// <summary>
    /// Зарегистрировать функционала поставки трассировок.
    /// </summary>
    /// <param name="services">Коллекция служб.</param>
    internal static void AddTracingInternal(this IServiceCollection services)
    {
        services
            .AddOpenTelemetry()
            .WithTracing(tracerProviderBuilder =>
            {
                tracerProviderBuilder.AddAspNetCoreInstrumentation();
                tracerProviderBuilder.ConfigureResource(resourceBuilder =>
                    resourceBuilder.AddService(typeof(Program).Assembly.GetName().Name ?? "rsse"));
                tracerProviderBuilder.AddConsoleExporter();
            });
    }
}
#endif
