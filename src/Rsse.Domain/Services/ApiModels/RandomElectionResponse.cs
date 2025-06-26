using System.Text.Json.Serialization;
using SearchEngine.Services.Configuration;

namespace SearchEngine.Services.ApiModels;

/// <summary>
/// Контракт ответа изменения алгоритма выбора.
/// </summary>
/// <param name="ElectionType">Флаг случайного выбора следующей заметки.</param>
public record RandomElectionResponse
(
    [property: JsonPropertyName("electionType")] ElectionType ElectionType
);
