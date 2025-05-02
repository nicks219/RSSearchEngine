using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SearchEngine.Domain.Dto;

/// <summary>
/// Шаблон передачи данных для заметки
/// </summary>
public record NoteDto
{
    /// <summary>
    /// Список отмеченных тегов в запросе
    /// </summary>
    public List<int>? TagsCheckedRequest { get; set; }

    /// <summary>
    /// Именование заметки в запросе
    /// </summary>
    public string? TitleRequest { get; set; }

    /// <summary>
    /// Текст заметки в запросе
    /// </summary>
    // todo: сделай init и поправь ошибки
    public string? TextRequest { get; set; }

    /// <summary>
    /// Представление списка тегов в виде строк "отмечено-не отмечено" в ответе
    /// </summary>
    public List<string>? TagsCheckedUncheckedResponse { get; init; }

    /// <summary>
    /// Именование заметки в ответе
    /// </summary>
    public string? TitleResponse { get; set; }

    /// <summary>
    /// Текст заметки в ответе
    /// </summary>
    public string? TextResponse { get; set; }

    /// <summary>
    /// Список тегов в формате "имя : количество записей"
    /// </summary>
    public List<string>? StructuredTagsListResponse { get; init; }

    /// <summary>
    /// Поле для хранения идентификатора сохраненной/измененной заметки
    /// </summary>
    public int CommonNoteId { get; set; }

    /// <summary>
    /// Сообщение об ошибке
    /// </summary>
    public string? CommonErrorMessageResponse { get; set; }

    /// <summary>
    /// Создать незаполненный шаблон передачи данных для заметки
    /// </summary>
    public NoteDto()
    {
    }

    /// <summary>
    /// Создать шаблон передачи данных для заметки
    /// </summary>
    // todo: это response
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
