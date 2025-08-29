using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Rsse.Domain.Service.ApiModels;

namespace Rsse.Domain.Service.Converters;

/// <summary>
/// Сериализация обертки с коллекцией от KeyValuePair в формат словаря.
/// </summary>
public class ComplianceMetricsListConverter : JsonConverter<ComplianceMetricsListResponse>
{
    public override void Write(Utf8JsonWriter writer, ComplianceMetricsListResponse metrics, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        foreach (var kvp in metrics)
        {
            writer.WritePropertyName(kvp.Key.ToString());
            writer.WriteNumberValue(kvp.Value);
        }
        writer.WriteEndObject();
    }

    public override ComplianceMetricsListResponse Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var dictionary = JsonSerializer.Deserialize<Dictionary<int, double>>(ref reader, options);
        return new ComplianceMetricsListResponse(dictionary?.Select(kv => new KeyValuePair<int, double>(kv.Key, kv.Value)) ?? []);
    }
}
