using System.Collections.Generic;
using System.Text.Json.Serialization;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

namespace SearchEngine.Domain.ApiModels;

/// <summary>
/// Шаблон передачи данных каталога
/// </summary>
public record CatalogRequest
(
    // <summary/> Номер страницы каталога
    [property: JsonPropertyName("pageNumber")] int PageNumber,
    // <summary/> Направление перемещения по каталогу
    [property: JsonPropertyName("direction")] List<int> Direction
);
