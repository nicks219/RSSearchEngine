using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SearchEngine.Data.Dto;

/// <summary>
/// Шаблон передачи данных авторизации
/// </summary>
public record NoteDto
{
    /// <summary>
    /// Список отмеченных тегов в запросе
    /// </summary>
    [JsonPropertyName("checkedCheckboxesJs")]
    public List<int>? TagsCheckedRequest { get; set; }

    /// <summary>
    /// Именование заметки в запросе
    /// </summary>
    [JsonPropertyName("titleJs")]
    public string? TitleRequest { get; set; }

    /// <summary>
    /// Текст заметки в запросе
    /// </summary>
    [JsonPropertyName("textJs")]
    public string? TextRequest { get; set; }

    /// <summary>
    /// Представление списка тегов в виде строк "отмечено-не отмечено" в ответе
    /// </summary>
    [JsonPropertyName("isGenreCheckedCS")]
    public List<string>? TagsCheckedUncheckedResponse { get; init; }

    /// <summary>
    /// Именование заметки в ответе
    /// </summary>
    [JsonPropertyName("titleCS")]
    public string? TitleResponse { get; set; }

    /// <summary>
    /// Текст заметки в ответе
    /// </summary>
    [JsonPropertyName("textCS")]
    public string? TextResponse { get; set; }

    // common:
    [JsonPropertyName("genresNamesCS")]
    public List<string>? CommonTagsListResponse { get; init; }

    /// <summary>
    /// Поле для хранения идентификатора сохраненной/измененной заметки
    /// </summary>
    [JsonPropertyName("savedTextId")]
    public int CommonNoteId { get; set; }

    /// <summary>
    /// Сообщение об ошибке
    /// </summary>
    public string? CommonErrorMessageResponse { get; set; }

    /// <summary>
    /// Создать незаполненный шаблон передачи данных авторизации
    /// </summary>
    public NoteDto()
    {
    }

    /// <summary>
    /// Создать шаблон передачи данных авторизации
    /// </summary>
    public NoteDto(
        List<string> commonTagsListResponse,
        int commonNoteId = 0,
        string textResponse = "",
        string titleResponse = "",
        List<string>? tagsCheckedUncheckedResponse = null)
    {
        TextResponse = textResponse;
        TitleResponse = titleResponse;
        TagsCheckedUncheckedResponse = tagsCheckedUncheckedResponse ?? new List<string>();
        CommonTagsListResponse = commonTagsListResponse;
        CommonNoteId = commonNoteId;
    }
}
