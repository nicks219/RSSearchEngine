using System.Collections.Generic;
using System.Text.Json.Serialization;
using SearchEngine.Domain.Dto;

namespace SearchEngine.Domain.ApiModels;

/// <summary>
/// Ответ каталога со страницей.
/// </summary>
/// <param name="CatalogPage">Названия заметок и соответствующие им Id</param>
/// <param name="NotesCount">Количество заметок</param>
/// <param name="PageNumber">Номер страницы каталога</param>
public record CatalogResponse
(
    [property: JsonPropertyName("catalogPage")] List<CatalogItemDto>? CatalogPage,
    [property: JsonPropertyName("notesCount")] int NotesCount,
    [property: JsonPropertyName("pageNumber")] int PageNumber
) : CatalogBaseResponse;

/// <summary>
/// Ответ каталога с ошибкой.
/// </summary>
public record CatalogErrorResponse : CatalogBaseResponse
{
    [JsonPropertyName("errorMessage")] public string? ErrorMessage { get; init; }
}

/// <summary>
/// Маркер ответа ручек каталога.
/// </summary>
public record CatalogBaseResponse;
