using System.Text.Json.Serialization;

namespace Rsse.Domain.Service.Configuration;

/// <summary>
/// Алгоритм выбора следующей заметки.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ElectionType
{
    SqlRandom = 0,
    Rng = 1,
    RoundRobin = 2,
    Unique = 3
}
