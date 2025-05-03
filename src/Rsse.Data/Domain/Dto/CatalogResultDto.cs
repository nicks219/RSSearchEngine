using System;
using System.Collections.Generic;

namespace SearchEngine.Domain.Dto;

/// <summary>
/// Шаблон передачи данных каталога
/// </summary>
public record struct CatalogResultDto
{
    /// <summary>
    /// Названия заметок и соответствующие им Id
    /// </summary>
    public List<CatalogResult>? CatalogPage { get; init; }

    /// <summary>
    /// Количество заметок
    /// </summary>
    public int NotesCount { get; init; }

    /// <summary>
    /// Номер страницы каталога
    /// </summary>
    public int PageNumber { get; init; }

    /// <summary>
    /// Сообщение об ошибке, если потребуется
    /// </summary>
    public string? ErrorMessage { get; init; }
}
