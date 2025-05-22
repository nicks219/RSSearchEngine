using System.Text.Json.Serialization;

namespace SearchEngine.Service.ApiModels;

/// <summary>
/// Контракт запроса обновления данных авторизации.
/// </summary>
/// <param name="OldCredos">Существующие данные авторизации.</param>
/// <param name="NewCredos">Новые данные авторизации.</param>
public record UpdateCredentialsRequest(
    [property: JsonPropertyName("OldCredos")] CredentialsRequest OldCredos,
    [property: JsonPropertyName("NewCredos")] CredentialsRequest NewCredos
);
