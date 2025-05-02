using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SearchEngine.Domain.ApiModels;

public record NoteResponse
{
    /// <summary/> Представление списка тегов в виде строк "отмечено-не отмечено" в ответе
    [JsonPropertyName("tagsCheckedUncheckedResponse")] public List<string>? TagsCheckedUncheckedResponse { get; init; }

    /// <summary/> Именование заметки в ответе
    [JsonPropertyName("titleResponse")] public string? TitleResponse { get; init; }

    /// <summary/> Текст заметки в ответе
    [JsonPropertyName("textResponse")] public string? TextResponse { get; init; }

    /// <summary/> Список тегов в формате "имя : количество записей"
    [JsonPropertyName("structuredTagsListResponse")] public List<string>? StructuredTagsListResponse { get; init; }

    /// <summary/> Поле для передачи идентификатора сохраненной/измененной заметки в обе стороны
    [JsonPropertyName("commonNoteID")] public int NoteIdExchange { get; init; }

    /// <summary/> Сообщение об ошибке
    [JsonPropertyName("errorMessageResponse")] public string? CommonErrorMessageResponse { get; init; }
}
