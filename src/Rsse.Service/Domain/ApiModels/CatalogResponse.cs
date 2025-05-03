using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using SearchEngine.Domain.Dto;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

namespace SearchEngine.Domain.ApiModels;

// todo: выделить ErrorResponse
public record CatalogResponse
{
    // <summary/> Названия заметок и соответствующие им Id
    [JsonPropertyName("catalogPage")] public List<CatalogResult>? CatalogPage { get; init; }

    // <summary/> Количество заметок
    [JsonPropertyName("notesCount")] public int NotesCount { get; init; }

    // <summary/> Номер страницы каталога
    [JsonPropertyName("pageNumber")] public int PageNumber { get; init; }

    // <summary/> Сообщение об ошибке, если потребуется
    [JsonPropertyName("errorMessage")] public string? ErrorMessage { get; init; }
}
