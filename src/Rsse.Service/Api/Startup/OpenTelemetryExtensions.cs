#if TRACING_ENABLE
using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Resources;
using SearchEngine.Service.Configuration;
using Serilog;

namespace SearchEngine.Api.Startup;

/// <summary>
/// Расширение для функционала поставки диагностиков.
/// </summary>
internal static class OpenTelemetryExtensions
{
    /// <summary>
    /// Зарегистрировать функционала поставки диагностиков.
    /// </summary>
    /// <param name="services">Коллекция служб.</param>
    /// <param name="configuration">Конфигурация.</param>
    internal static void AddMetricsAndTracingInternal(this IServiceCollection services, IConfiguration configuration)
    {
        var otlpEndpoint = configuration.GetValue<string>("Otlp:Endpoint");
        var podName = Environment.GetEnvironmentVariable("POD_NAME") ?? "uncloud";
        Log.ForContext<Startup>().Information("OTLP: `{Endpoint}` | pod: `{PodName}`", otlpEndpoint, podName);

        services
            .AddOpenTelemetry()
            .ConfigureResource(resource => resource
                // далее service name оверрайдится на Rsse.Service
                .AddService(serviceName: Constants.ServiceName,
                    serviceNamespace: Constants.ServiceNamespace,
                    serviceVersion: Constants.ApplicationVersion,
                    serviceInstanceId: podName
                // autoGenerateServiceInstanceId: true
                )
                .AddAttributes([new KeyValuePair<string, object>("deployment.environment", "production")]))
            .WithTracingInternal(otlpEndpoint)
            .WithMetricsInternal(otlpEndpoint);
    }
}
#endif
