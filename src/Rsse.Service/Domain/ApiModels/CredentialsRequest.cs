using System.Text.Json.Serialization;

// ReSharper disable ClassNeverInstantiated.Global

namespace SearchEngine.Domain.ApiModels;

/// <summary>
/// Контракт запроса авторизации.
/// </summary>
public record CredentialsRequest
(
    [property: JsonPropertyName("email")] string Email,
    [property: JsonPropertyName("password")] string Password
);
