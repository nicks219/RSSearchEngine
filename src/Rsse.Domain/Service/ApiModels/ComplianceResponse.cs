using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SearchEngine.Service.ApiModels;

/// <summary>
/// Контракт ответа ручки получения индекса релевантности.
/// </summary>
/// <param name="Res">Словарь с результатами поиска.</param>
/// <param name="Error">Строка с ошибкой.</param>
public record ComplianceResponse
(
    [property: JsonPropertyName("res")] Dictionary<int, double>? Res = null,
    [property: JsonPropertyName("error")] string? Error = null
);
