using System;
using OpenTelemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Trace;
using SearchEngine.Service.Configuration;
using Serilog;

namespace SearchEngine.Api.Startup;

/// <summary>
/// Расширение функционала поставки трассировок.
/// </summary>
public static class TracingInternal
{
    /// <summary>
    /// Добавить функционал поставки трасс.
    /// </summary>
    /// <param name="builder">Конфигуратор для диагностиков.</param>
    /// <param name="otlpEndpoint">Эндпоинт OTLP-коллектора.</param>
    internal static OpenTelemetryBuilder WithTracingInternal(this OpenTelemetryBuilder builder, string? otlpEndpoint)
    {
        return builder
            .WithTracing(tracerProviderBuilder =>
            {
                if (Environment.GetEnvironmentVariable(Constants.AspNetCoreOtlpExportersDisable) == Constants.DisableValue)
                {
                    return;
                }

                tracerProviderBuilder.AddAspNetCoreInstrumentation(options =>
                {
                    options.Filter = context =>
                        !context.Request.Path.StartsWithSegments("/system");
                });

                if (otlpEndpoint == null) throw new Exception("Otlp:Endpoint not found.");

                tracerProviderBuilder.AddOtlpExporter(options =>
                {
                    options.Endpoint = new Uri(otlpEndpoint);
                    options.Protocol = OtlpExportProtocol.Grpc;
                });
                Log.ForContext<Startup>().Information("OTLP exporter for tracing was added");
            });
    }
}
