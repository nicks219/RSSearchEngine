namespace SearchEngine.Domain.Dto;

/// <summary>
/// Контейнер с записью в странице каталога.
/// </summary>
public record CatalogItemDto
{
    public required string Title { get; init; }
    public required int NoteId { get; init; }
}
