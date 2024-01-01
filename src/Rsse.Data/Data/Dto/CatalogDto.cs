using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SearchEngine.Data.Dto;

/// <summary>
/// Шаблон передачи данных каталога
/// </summary>
public record CatalogDto
{
    private const int Backward = 1;
    private const int Forward = 2;

    /// <summary>
    /// Название и соответствующее ему Id из бд
    /// </summary>
    [JsonPropertyName("titlesAndIds")]
    public List<Tuple<string, int>>? CatalogPage { get; init; }

    /// <summary>
    /// Количество заметок
    /// </summary>
    [JsonPropertyName("songsCount")]
    public int NotesCount { get; init; }

    /// <summary>
    /// Номер страницы каталога
    /// </summary>
    [JsonPropertyName("pageNumber")]
    public int PageNumber { get; init; }

    /// <summary>
    /// Сообщение об ошибке, если потребуется
    /// </summary>
    [JsonPropertyName("errorMessage")]
    public string? ErrorMessage { get; init; }

    /// <summary>
    /// Направление перемещения по каталогу
    /// </summary>
    [JsonPropertyName("navigationButtons")]
    public List<int>? NavigationButtons { get; init; }

    /// <summary>
    /// Получить направление перемещения по каталогу в виде константы
    /// </summary>
    public int GetDirection()
    {
        if (NavigationButtons is null)
        {
            return 0;
        }

        return NavigationButtons[0] switch
        {
            Backward => Backward,
            Forward => Forward,
            _ => throw new NotImplementedException($"[{nameof(GetDirection)}] unknown direction")
        };
    }
}
