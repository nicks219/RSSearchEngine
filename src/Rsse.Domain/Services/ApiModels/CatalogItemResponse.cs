using System.Text.Json.Serialization;

namespace SearchEngine.Services.ApiModels;

/// <summary>
/// Контейнер с одной записью в странице каталога, для контракта ответа со страницей каталога.
/// </summary>
/// <param name="Title">Именование заметки.</param>
/// <param name="NoteId">Идентификатор заметки.</param>
public record CatalogItemResponse
(
    [property: JsonPropertyName("item1")] string Title,
    [property: JsonPropertyName("item2")] int NoteId
);
