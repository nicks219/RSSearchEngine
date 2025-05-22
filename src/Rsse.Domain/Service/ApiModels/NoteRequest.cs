using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SearchEngine.Service.ApiModels;

/// <summary>
/// Контракт запроса с заметкой.
/// </summary>
/// <param name="CheckedTags">Список отмеченных тегов в запросе.</param>
/// <param name="Title">Именование заметки в запросе.</param>
/// <param name="Text">Текст заметки в запросе.</param>
/// <param name="NoteIdExchange">Поле для передачи идентификатора сохраненной/измененной заметки в обе стороны.</param>
public record NoteRequest
(
    [property: JsonPropertyName("tagsCheckedRequest")] List<int>? CheckedTags = null,
    [property: JsonPropertyName("titleRequest")] string? Title = null,
    [property: JsonPropertyName("textRequest")] string? Text = null,
    [property: JsonPropertyName("commonNoteID")] int? NoteIdExchange = null
);
