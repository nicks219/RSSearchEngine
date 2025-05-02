using System.Text.Json.Serialization;

namespace SearchEngine.Domain.ApiModels;

/// <summary>
/// Контейнер для обновления данных авторизации
/// </summary>
public record UpdateCredosRequest
{
    [JsonPropertyName("OldCredos")] public required LoginDtoRequest OldCredos { get; init; }
    [JsonPropertyName("NewCredos")] public required LoginDtoRequest NewCredos { get; init; }
}
