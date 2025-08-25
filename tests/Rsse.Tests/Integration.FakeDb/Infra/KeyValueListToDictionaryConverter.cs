using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Rsse.Tests.Integration.FakeDb.Infra;

/// <summary>
/// Поддержка сериализации List от KeyValuePair в формате словаря.
/// </summary>
public class KeyValueListToDictionaryConverter<TKey, TValue> : JsonConverter<List<KeyValuePair<TKey, TValue>>> where TKey : notnull
{
    // Сериализация.
    public override void Write(Utf8JsonWriter writer, List<KeyValuePair<TKey, TValue>> list, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        foreach (var kvp in list)
        {
            var name = kvp.Key.ToString() ?? "NRE";
            writer.WritePropertyName(name);
            JsonSerializer.Serialize(writer, kvp.Value, options);
        }
        writer.WriteEndObject();
    }

    // Десериализация.
    public override List<KeyValuePair<TKey, TValue>> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var dictionary = JsonSerializer.Deserialize<Dictionary<TKey, TValue>>(ref reader, options);
        return dictionary?.Select(kv => new KeyValuePair<TKey, TValue>(kv.Key, kv.Value)).ToList() ?? [];
    }
}
