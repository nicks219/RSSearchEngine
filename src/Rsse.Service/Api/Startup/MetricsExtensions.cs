#if TRACING_ENABLE
using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Metrics;
using SearchEngine.Service.Configuration;

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
    /// <param name="configuration">Конфигурация.</param>
    internal static void AddMetricsInternal(this IServiceCollection services, IConfiguration configuration)
    {
        if (Environment.GetEnvironmentVariable(Constants.AspNetCoreEnvironmentName) == Constants.TestingEnvironment)
        {
            return;
        }

        var endpoint = configuration.GetValue<string>("Otlp:Endpoint");
        if (endpoint == null) throw new Exception("Otlp:Endpoint not found.");

        services.AddOpenTelemetry()
            .WithMetrics(meterProviderBuilder =>
            {
                // todo: подумать, стоит ли удалить после настройки OTLP (в тч зависимость OpenTelemetry.Exporter)
                meterProviderBuilder.AddPrometheusExporter();
                meterProviderBuilder.AddOtlpExporter(options =>
                {
                    options.Endpoint = new Uri(endpoint);
                });
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
#endif
