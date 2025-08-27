using System.Text.Json.Serialization;
using Rsse.Domain.Data.Configuration;
using Rsse.Domain.Service.Configuration;
using Rsse.Domain.Service.Converters;

namespace Rsse.Api.Startup;

/// <summary>
/// Контекст для кодогенерации сериализатора.
/// </summary>
[JsonSerializable(typeof(DatabaseType))]
[JsonSerializable(typeof(ElectionType))]
[JsonSerializable(typeof(ComplianceMetricsListConverter))]
public partial class AppJsonContext : JsonSerializerContext { }
