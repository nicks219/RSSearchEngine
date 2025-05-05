using System.Text.Json.Serialization;

namespace SearchEngine.Domain.Dto;

/// <summary>
/// Контейнер с записью в странице каталога
/// </summary>
// todo: отрефактори, опять "универсальный объект" получился, тк присутствует в CatalogResultDto и CatalogResponse
public record CatalogItemDto
{
    [JsonPropertyName("item1")] public required string Title { get; init; }
    [JsonPropertyName("item2")] public required int NoteId { get; init; }
}
