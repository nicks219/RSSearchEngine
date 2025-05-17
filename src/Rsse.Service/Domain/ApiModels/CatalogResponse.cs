using System.Collections.Generic;
using System.Text.Json.Serialization;
using SearchEngine.Domain.Dto;

namespace SearchEngine.Domain.ApiModels;

public record CatalogResponse
(
    // Названия заметок и соответствующие им Id
    [property: JsonPropertyName("catalogPage")]  List<CatalogItemDto>? CatalogPage,
    // Количество заметок
    [property: JsonPropertyName("notesCount")] int NotesCount,
    // Номер страницы каталога
    [property: JsonPropertyName("pageNumber")] int PageNumber
) : CatalogBaseResponse;

public record CatalogBaseResponse
{
    [JsonPropertyName("errorMessage")] public string? ErrorMessage { get; init; }
}
