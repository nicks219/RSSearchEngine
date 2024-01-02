using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SearchEngine.Data.Dto;

/// <summary>
/// Шаблон передачи данных для заметки
/// </summary>
public record NoteDto
{
    /// <summary>
    /// Список отмеченных тегов в запросе
    /// </summary>
    [JsonPropertyName("tagsCheckedRequest")] public List<int>? TagsCheckedRequest { get; set; }

    /// <summary>
    /// Именование заметки в запросе
    /// </summary>
    [JsonPropertyName("titleRequest")] public string? TitleRequest { get; set; }

    /// <summary>
    /// Текст заметки в запросе
    /// </summary>
    [JsonPropertyName("textRequest")] public string? TextRequest { get; set; }

    /// <summary>
    /// Представление списка тегов в виде строк "отмечено-не отмечено" в ответе
    /// </summary>
    [JsonPropertyName("tagsCheckedUncheckedResponse")] public List<string>? TagsCheckedUncheckedResponse { get; init; }

    /// <summary>
    /// Именование заметки в ответе
    /// </summary>
    [JsonPropertyName("titleResponse")] public string? TitleResponse { get; set; }

    /// <summary>
    /// Текст заметки в ответе
    /// </summary>
    [JsonPropertyName("textResponse")] public string? TextResponse { get; set; }

    /// <summary>
    /// Список тегов в формате "имя : количество записей"
    /// </summary>
    [JsonPropertyName("structuredTagsListResponse")] public List<string>? StructuredTagsListResponse { get; init; }

    /// <summary>
    /// Поле для хранения идентификатора сохраненной/измененной заметки
    /// </summary>
    [JsonPropertyName("commonNoteID")] public int CommonNoteId { get; set; }

    /// <summary>
    /// Сообщение об ошибке
    /// </summary>
    [JsonPropertyName("errorMessageResponse")] public string? CommonErrorMessageResponse { get; set; }

    /// <summary>
    /// Создать незаполненный шаблон передачи данных для заметки
    /// </summary>
    public NoteDto()
    {
    }

    /// <summary>
    /// Создать шаблон передачи данных для заметки
    /// </summary>
    public NoteDto(
        List<string> structuredTagsListResponse,
        int commonNoteId = 0,
        string textResponse = "",
        string titleResponse = "",
        List<string>? tagsCheckedUncheckedResponse = null)
    {
        TextResponse = textResponse;
        TitleResponse = titleResponse;
        TagsCheckedUncheckedResponse = tagsCheckedUncheckedResponse ?? new List<string>();
        StructuredTagsListResponse = structuredTagsListResponse;
        CommonNoteId = commonNoteId;
    }
}
