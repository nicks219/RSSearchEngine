using System.Text.Json.Serialization;

// ReSharper disable ClassNeverInstantiated.Global

namespace SearchEngine.Domain.ApiModels;

/// <summary>
/// Контракт запроса данных авторизации.
/// </summary>
/// <param name="Email">Электронная почта.</param>
/// <param name="Password">Пароль.</param>
public record CredentialsRequest
(
    [property: JsonPropertyName("email")] string Email,
    [property: JsonPropertyName("password")] string Password
);
