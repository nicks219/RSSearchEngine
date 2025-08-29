using System.Text.Json.Serialization;

namespace Rsse.Domain.Service.ApiModels;

/// <summary>
/// Контракт ответа ручки получения индекса релевантности.
/// </summary>
/// <param name="Res">Словарь с результатами поиска.</param>
/// <param name="Error">Строка с ошибкой.</param>
public record ComplianceResponse
(
    [property: JsonPropertyName("res")] ComplianceMetricsListResponse? Res = null,
    [property: JsonPropertyName("error")] string? Error = null
);
