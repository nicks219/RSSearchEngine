using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SearchEngine.Domain.ApiModels;

/// <summary>
/// Контракт запроса данных для заметки.
/// </summary>
public record NoteRequest
{
    /// <summary/> Список отмеченных тегов в запросе
    [JsonPropertyName("tagsCheckedRequest")] public List<int>? CheckedTags { get; init; }

    /// <summary/> Именование заметки в запросе
    [JsonPropertyName("titleRequest")] public string? Title { get; init; }

    /// <summary/> Текст заметки в запросе
    [JsonPropertyName("textRequest")] public string? Text { get; init; }

    /// <summary/> Поле для передачи идентификатора сохраненной/измененной заметки в обе стороны
    [JsonPropertyName("commonNoteID")] public int NoteIdExchange { get; init; }
}
