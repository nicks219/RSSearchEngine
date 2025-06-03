#if TRACING_ENABLE
using System;
using OpenTelemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;
using SearchEngine.Service.Configuration;
using Serilog;

namespace SearchEngine.Api.Startup;

/// <summary>
/// Расширение функционала поставки метрик.
/// </summary>
internal static class MetricsExtensions
{
    /// <summary>
    /// Добавить функционал поставки метрик.
    /// </summary>
    /// <param name="builder">Конфигуратор для диагностиков.</param>
    /// <param name="otlpEndpoint">Эндпоинт OTLP-коллектора.</param>
    internal static OpenTelemetryBuilder WithMetricsInternal(this OpenTelemetryBuilder builder, string? otlpEndpoint)
    {
        return builder
            .WithMetrics(meterProviderBuilder =>
            {
                // todo: подумать, стоит ли удалить после настройки OTLP (в тч зависимость OpenTelemetry.Exporter)
                // meterProviderBuilder.AddPrometheusExporter();

                if (Environment.GetEnvironmentVariable(Constants.AspNetCoreObservabilityDisableName) !=
                    Constants.DisableValue)
                {
                    if (otlpEndpoint == null) throw new Exception("Otlp:Endpoint not found.");
                    meterProviderBuilder.AddOtlpExporter(options =>
                    {
                        options.Endpoint = new Uri(otlpEndpoint);
                        options.Protocol = OtlpExportProtocol.Grpc;
                        Log.ForContext<Startup>().Information("OTLP exporter for metrics was added");
                    });
                }

                // kestrel_connection_duration_seconds_bucket
                // Microsoft.AspNetCore.Server.Kestrel: 0.01 , 0.02 , 0.05 , 0.1 , 0.2 , 0.5 , 1 , 2 , 5 , 10 , 30 , 60 , 120 , 300
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

