using System.Collections.Generic;

namespace Rsse.Domain.Data.Dto;

/// <summary>
/// Контейнер запроса перемещения по каталогу.
/// </summary>
public record struct CatalogRequestDto
{
    /// <summary>
    /// Номер текущей страницы каталога.
    /// </summary>
    public int PageNumber { get; init; }

    /// <summary>
    /// Направление перемещения по каталогу.
    /// </summary>
    public List<int> Direction { get; init; }
}
