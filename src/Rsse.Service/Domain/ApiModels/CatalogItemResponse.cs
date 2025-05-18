using System.Text.Json.Serialization;

namespace SearchEngine.Domain.ApiModels;

/// <summary>
/// Контейнер с записью в странице каталога для контракта ответа со страницей каталога.
/// </summary>
// ReSharper disable once ClassNeverInstantiated.Global
public record CatalogItemResponse
{
    [JsonPropertyName("item1")] public required string Title { get; init; }
    [JsonPropertyName("item2")] public required int NoteId { get; init; }
}
