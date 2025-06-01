#if TRACING_ENABLE
using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
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
    /// <param name="configuration">Конфигурация.</param>
    internal static void AddTracingInternal(this IServiceCollection services, IConfiguration configuration)
    {
        var endpoint = configuration.GetValue<string>("Otlp:Endpoint");
        if (endpoint == null) throw new Exception("Otlp:Endpoint not found.");

        services
            .AddOpenTelemetry()
            .ConfigureResource(resource => resource
                // service name заоверрайдится на Rsse.Service
                .AddService("rsse-app", serviceNamespace: "rsse-group")
                .AddAttributes([
                    new KeyValuePair<string, object>("deployment.environment", "production")
                ]))
            .WithTracing(tracerProviderBuilder =>
            {
                tracerProviderBuilder.AddAspNetCoreInstrumentation();
                tracerProviderBuilder.ConfigureResource(resourceBuilder =>
                    resourceBuilder.AddService(typeof(Program).Assembly.GetName().Name ?? "rsse"));
                // todo: удалить после настройки OTLP (в тч зависимость OpenTelemetry.Exporter)
                tracerProviderBuilder.AddConsoleExporter();
                tracerProviderBuilder.AddOtlpExporter(options =>
                {
                    options.Endpoint = new Uri(endpoint);
                });
            });
    }
}
#endif
/* можеь подхватить:
"OpenTelemetry": {
    "Otlp": {
      "Endpoint": "http://otel-collector:4317",
      "Protocol": "Grpc" // или "HttpProtobuf"
    }
  }
  */
