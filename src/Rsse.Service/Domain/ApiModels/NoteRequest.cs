using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SearchEngine.Domain.ApiModels;

/// <summary>
/// Шаблон передачи данных для заметки
/// </summary>
public record NoteRequest
{
    /// <summary>
    /// Список отмеченных тегов в запросе
    /// </summary>
    [JsonPropertyName("tagsCheckedRequest")] public List<int>? TagsCheckedRequest { get; init; }

    /// <summary>
    /// Именование заметки в запросе
    /// </summary>
    [JsonPropertyName("titleRequest")] public string? TitleRequest { get; init; }

    /// <summary>
    /// Текст заметки в запросе
    /// </summary>
    [JsonPropertyName("textRequest")] public string? TextRequest { get; init; }

    /// <summary>
    /// Представление списка тегов в виде строк "отмечено-не отмечено" в ответе
    /// </summary>
    [JsonPropertyName("tagsCheckedUncheckedResponse")] public List<string>? TagsCheckedUncheckedResponse { get; init; }

    /// <summary>
    /// Именование заметки в ответе
    /// </summary>
    [JsonPropertyName("titleResponse")] public string? TitleResponse { get; init; }

    /// <summary>
    /// Текст заметки в ответе
    /// </summary>
    [JsonPropertyName("textResponse")] public string? TextResponse { get; init; }

    /// <summary>
    /// Список тегов в формате "имя : количество записей"
    /// </summary>
    [JsonPropertyName("structuredTagsListResponse")] public List<string>? StructuredTagsListResponse { get; init; }

    /// <summary>
    /// Поле для хранения идентификатора сохраненной/измененной заметки
    /// </summary>
    [JsonPropertyName("commonNoteID")] public int CommonNoteId { get; init; }

    /// <summary>
    /// Сообщение об ошибке
    /// </summary>
    [JsonPropertyName("errorMessageResponse")] public string? CommonErrorMessageResponse { get; init; }
}
