using System.Collections.Generic;

namespace SearchEngine.Data.Dto;

/// <summary>
/// Контейнер с ответом, содержащий страницу каталога.
/// </summary>
public record struct CatalogResultDto
{
    /// <summary>
    /// Названия заметок и соответствующие им идентификаторы.
    /// </summary>
    public List<CatalogItemDto>? CatalogPage { get; init; }

    /// <summary>
    /// Общее количество заметок в сервисе.
    /// </summary>
    public int NotesCount { get; init; }

    /// <summary>
    /// Номер страницы каталога.
    /// </summary>
    public int PageNumber { get; init; }

    /// <summary>
    /// Сообщение об ошибке, если требуется.
    /// </summary>
    public string? ErrorMessage { get; init; }
}
