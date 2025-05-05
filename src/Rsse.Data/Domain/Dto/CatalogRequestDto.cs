using System.Collections.Generic;

namespace SearchEngine.Domain.Dto;

/// <summary>
/// Шаблон передачи данных каталога
/// </summary>
public record struct CatalogRequestDto
{

    /// <summary>
    /// Номер страницы каталога
    /// </summary>
    public int PageNumber { get; init; }

    /// <summary>
    /// Направление перемещения по каталогу
    /// </summary>
    public List<int> Direction { get; init; }
}
