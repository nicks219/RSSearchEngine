using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using RsseEngine.Dto;

namespace Rsse.Tests.Integration.FakeDb.Infra;

/// <summary>
/// Поддержка сериализации DocumentId для использования в качестве ключа словаря.
/// </summary>
public class DocumentIdJsonConverter : JsonConverter<DocumentId>
{
    // Десериализация значения.
    public override DocumentId Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return reader.TokenType switch
        {
            JsonTokenType.String => new DocumentId(int.Parse(reader.GetString()!)),
            JsonTokenType.Number => new DocumentId(reader.GetInt32()),
            _ => throw new JsonException($"Unexpected token type {reader.TokenType} when parsing DocumentId.")
        };
    }

    // Сериализация значения.
    public override void Write(Utf8JsonWriter writer, DocumentId value, JsonSerializerOptions options)
    {
        writer.WriteNumberValue(value.Value);
    }

    // Десериализация ключа.
    public override DocumentId ReadAsPropertyName(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        // Ключ словаря в JSON всегда строка, даже если это число
        var keyString = reader.GetString();
        return new DocumentId(int.Parse(keyString!));
    }

    // Сериализация ключа.
    public override void WriteAsPropertyName(Utf8JsonWriter writer, DocumentId value, JsonSerializerOptions options)
    {
        writer.WritePropertyName(value.Value.ToString());
    }
}
