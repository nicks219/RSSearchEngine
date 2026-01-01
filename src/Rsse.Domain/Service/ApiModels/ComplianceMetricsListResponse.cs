using System.Collections.Generic;
using System.Text.Json.Serialization;
using Rsse.Domain.Service.Converters;

namespace Rsse.Domain.Service.ApiModels;

/// <summary>
/// Контракт поля ответа ручки получения индекса релевантности.
/// </summary>
[JsonConverter(typeof(ComplianceMetricsListConverter))]
public sealed class ComplianceMetricsListResponse : List<KeyValuePair<int, double>>
{
    public ComplianceMetricsListResponse() { }
    public ComplianceMetricsListResponse(IEnumerable<KeyValuePair<int, double>> collection) : base(collection) { }
}
