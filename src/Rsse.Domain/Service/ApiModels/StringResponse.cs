using System.Text.Json.Serialization;

namespace SearchEngine.Service.ApiModels;

/// <summary>
/// Контракт ответа со строками - контроллера миграций и ручки чтения.
/// </summary>
/// <param name="Res">Строка с информацией.</param>
/// <param name="Error">Строка с ошибкой.</param>
public record StringResponse
(
    [property: JsonPropertyName("res")] string? Res = null,
    [property: JsonPropertyName("error")] string? Error = null
);
