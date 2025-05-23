using System.Text.Json.Serialization;

namespace SearchEngine.Service.ApiModels;

/// <summary>
/// Контракт ответа изменения алгоритма выбора.
/// </summary>
/// <param name="RandomElection">Флаг случайного выбора следующей заметки.</param>
public record RandomElectionResponse
(
    [property: JsonPropertyName("randomElection")] bool RandomElection
);
