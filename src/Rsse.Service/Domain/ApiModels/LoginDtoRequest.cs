using System.Text.Json.Serialization;
// ReSharper disable ClassNeverInstantiated.Global

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

namespace SearchEngine.Domain.ApiModels;

/// <summary>
/// Шаблон передачи данных авторизации
/// </summary>
public record LoginDtoRequest
{
    [JsonPropertyName("email")] public required string Email { get; init; }

    [JsonPropertyName("password")] public required string Password { get; init; }
}
