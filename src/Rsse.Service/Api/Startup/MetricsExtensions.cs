#if TRACING_ENABLE
using System;
using System.Diagnostics.Metrics;
using OpenTelemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;
using SearchEngine.Services.Configuration;
using Serilog;

namespace SearchEngine.Api.Startup;

/// <summary>
/// Расширение функционала поставки метрик.
/// </summary>
internal static class MetricsExtensions
{
    private static readonly Meter CustomMeter = new("exemplar.metrics", "1.0.0");
    private const string HistogramWithExemplarName = "http.server.duration_with_trace";
    internal static readonly Histogram<double> HistogramWithExemplar =
        CustomMeter.CreateHistogram<double>(HistogramWithExemplarName);

    private static readonly ExplicitBucketHistogramConfiguration CustomBuckets = new()
    {
        Boundaries = [0, 0.005, 0.01, 0.025, 0.05, 0.075, 0.1, 0.25, 0.5, 0.75, 1, 2.5, 5, 7.5, 10]
    };

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
                if (Environment.GetEnvironmentVariable(Constants.AspNetCoreOtlpExportersDisable) == Constants.DisableValue)
                {
                    return;
                }

                if (otlpEndpoint == null) throw new Exception("Otlp:Endpoint not found.");

                // kestrel_connection_duration_seconds_bucket
                // Microsoft.AspNetCore.Server.Kestrel: [0.01 , 0.02 , 0.05 , 0.1 , 0.2 , 0.5 , 1 , 2 , 5 , 10 , 30 , 60 , 120 , 300]
                meterProviderBuilder
                    .AddMeter("Microsoft.AspNetCore.Hosting", "Microsoft.AspNetCore.Server.Kestrel", "exemplar.metrics")
                    .AddView("http.server.request.duration", CustomBuckets)
                    .AddView(HistogramWithExemplarName, CustomBuckets)
                    .SetExemplarFilter(ExemplarFilterType.TraceBased)
                    // .AddPrometheusExporter()
                    .AddOtlpExporter(options =>
                    {
                        options.Endpoint = new Uri(otlpEndpoint);
                        options.Protocol = OtlpExportProtocol.Grpc;
                    });

                Log.ForContext<Startup>().Information("OTLP exporter for metrics was added");
            });
    }
}
#endif
