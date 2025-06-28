namespace Rsse.Domain.Data.Dto;

/// <summary>
/// Контейнер с записью в странице каталога.
/// </summary>
public record CatalogItemDto
{
    /// <summary>
    /// Название заметики.
    /// </summary>
    public required string Title { get; init; }

    /// <summary>
    /// Идентификатор заметки.
    /// </summary>
    public required int NoteId { get; init; }
}
