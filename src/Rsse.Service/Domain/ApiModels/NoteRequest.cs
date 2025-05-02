using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SearchEngine.Domain.ApiModels;

/// <summary>
/// Шаблон передачи данных для заметки
/// </summary>
public record NoteRequest
{
    /// <summary/> Список отмеченных тегов в запросе
    [JsonPropertyName("tagsCheckedRequest")] public List<int>? TagsCheckedRequest { get; init; }

    /// <summary/> Именование заметки в запросе
    [JsonPropertyName("titleRequest")] public string? TitleRequest { get; init; }

    /// <summary/> Текст заметки в запросе
    [JsonPropertyName("textRequest")] public string? TextRequest { get; init; }

    /// <summary/> Представление списка тегов в виде строк "отмечено-не отмечено" в ответе
    [JsonPropertyName("tagsCheckedUncheckedResponse")] public List<string>? TagsCheckedUncheckedResponse { get; init; }

    /// <summary/> Именование заметки в ответе
    [JsonPropertyName("titleResponse")] public string? TitleResponse { get; init; }

    /// <summary/> Текст заметки в ответе
    [JsonPropertyName("textResponse")] public string? TextResponse { get; init; }

    /// <summary/> Список тегов в формате "имя : количество записей"
    [JsonPropertyName("structuredTagsListResponse")] public List<string>? StructuredTagsListResponse { get; init; }

    /// <summary/> Поле для хранения идентификатора сохраненной/измененной заметки
    [JsonPropertyName("commonNoteID")] public int CommonNoteId { get; init; }

    /// <summary/> Сообщение об ошибке
    [JsonPropertyName("errorMessageResponse")] public string? CommonErrorMessageResponse { get; init; }
}
