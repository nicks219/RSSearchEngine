using System.Diagnostics;
using Serilog.Core;
using Serilog.Events;

namespace Rsse.Api.Observability;

/// <summary>
/// Функционал для явного обогащения логов идентификаторами трасс.
/// </summary>
public class ActivityEnricher : ILogEventEnricher
{
    /// <inheritdoc/>
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        var activity = Activity.Current;
        if (activity == null)
        {
            return;
        }

        logEvent.AddOrUpdateProperty(propertyFactory.CreateProperty("trace_id", activity.TraceId.ToHexString()));
        logEvent.AddOrUpdateProperty(propertyFactory.CreateProperty("span_id", activity.SpanId.ToHexString()));
    }
}
