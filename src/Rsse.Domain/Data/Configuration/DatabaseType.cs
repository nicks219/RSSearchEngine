using System.Text.Json.Serialization;

namespace Rsse.Domain.Data.Configuration;

/// <summary>
/// Тип бд либо мигратора.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum DatabaseType
{
    MySql = 0,
    Postgres = 1,
}
