using System.Collections.Generic;
using System.Text.Json.Serialization;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

namespace SearchEngine.Service.ApiModels;

/// <summary>
/// Контракт ответа для страницы каталога.
/// </summary>
/// <param name="ErrorMessage">Сообщение об ошибке, если потребуется.</param>
/// <param name="CatalogPage">Названия заметок и соответствующие им Id.</param>
/// <param name="NotesCount">Общее количество заметок сервиса.</param>
/// <param name="PageNumber">Номер страницы каталога.</param>
public record CatalogResponse(
    [property: JsonPropertyName("errorMessage")] string? ErrorMessage = null,
    [property: JsonPropertyName("catalogPage")] List<CatalogItemResponse>? CatalogPage = null,
    [property: JsonPropertyName("notesCount")] int? NotesCount = null,
    [property: JsonPropertyName("pageNumber")] int? PageNumber = null
);
