using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Metrics;

namespace SearchEngine.Api.Startup;

/// <summary>
/// Расширение функционала поставки метрик.
/// </summary>
internal static class MetricsExtensions
{
    /// <summary>
    /// Зарегистрировать функционал поставки метрик.
    /// </summary>
    /// <param name="services">Коллекция служб.</param>
    internal static void AddMetricsInternal(this IServiceCollection services)
    {
        services.AddOpenTelemetry()
            .WithMetrics(meterProviderBuilder =>
            {
                meterProviderBuilder.AddPrometheusExporter();
                meterProviderBuilder.AddMeter("Microsoft.AspNetCore.Hosting", "Microsoft.AspNetCore.Server.Kestrel");
                meterProviderBuilder.AddView("http.server.request.duration",
                    new ExplicitBucketHistogramConfiguration
                    {
                        Boundaries =
                        [
                            0,
                            0.005,
                            0.01,
                            0.025,
                            0.05,
                            0.075,
                            0.1,
                            0.25,
                            0.5,
                            0.75,
                            1,
                            2.5,
                            5,
                            7.5,
                            10
                        ]
                    });
            });
    }
}
