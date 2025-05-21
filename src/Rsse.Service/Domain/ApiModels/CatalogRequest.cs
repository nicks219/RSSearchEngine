using System.Collections.Generic;
using System.Text.Json.Serialization;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

namespace SearchEngine.Domain.ApiModels;

/// <summary>
/// Контракт запроса страницы каталога.
/// </summary>
/// <param name="PageNumber">Номер страницы каталога.</param>
/// <param name="Direction">Направление перемещения по каталогу.</param>
public record CatalogRequest
(
    [property: JsonPropertyName("pageNumber")] int PageNumber,
    [property: JsonPropertyName("direction")] List<int> Direction
);
