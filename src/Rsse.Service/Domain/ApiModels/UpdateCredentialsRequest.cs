using System.Text.Json.Serialization;

namespace SearchEngine.Domain.ApiModels;

/// <summary>
/// Контракт запроса обновления данных авторизации.
/// </summary>
public record UpdateCredentialsRequest(
    [property: JsonPropertyName("OldCredos")] CredentialsRequest OldCredos,
    [property: JsonPropertyName("NewCredos")] CredentialsRequest NewCredos
);
