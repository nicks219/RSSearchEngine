using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SearchEngine.Service.ApiModels;

/// <summary>
/// Контракт ответа с заметкой.
/// </summary>
/// <param name="CheckedUncheckedTags">Представление списка тегов в виде строк "отмечено-не отмечено" в ответе.</param>
/// <param name="Title">Именование заметки в ответе.</param>
/// <param name="Text">Текст заметки в ответе.</param>
/// <param name="StructuredTags">Список тегов в формате "имя : количество записей".</param>
/// <param name="NoteIdExchange">Поле для передачи идентификатора сохраненной/измененной заметки в обе стороны.</param>
/// <param name="ErrorMessage">Сообщение об ошибке.</param>
public record NoteResponse
(
    [property: JsonPropertyName("tagsCheckedUncheckedResponse")] List<string>? CheckedUncheckedTags = null,
    [property: JsonPropertyName("titleResponse")] string? Title = null,
    [property: JsonPropertyName("textResponse")] string? Text = null,
    [property: JsonPropertyName("structuredTagsListResponse")] List<string>? StructuredTags = null,
    [property: JsonPropertyName("commonNoteID")] int? NoteIdExchange = null,
    [property: JsonPropertyName("errorMessageResponse")] string? ErrorMessage = null
);
