using System.Collections.Generic;

namespace SearchEngine.Domain.Dto;

/// <summary>
/// Контейнер для ответа с заметкой.
/// </summary>
public record NoteResultDto
{
    /// <summary>
    /// Представление списка тегов в виде строк "отмечено-не отмечено" в ответе
    /// </summary>
    public List<string>? TagsCheckedUncheckedResponse { get; }

    /// <summary>
    /// Именование заметки в ответе
    /// </summary>
    public string? TitleResponse { get; init; }

    /// <summary>
    /// Текст заметки в ответе
    /// </summary>
    public string? TextResponse { get; init; }

    /// <summary>
    /// Список тегов в формате "имя : количество записей"
    /// </summary>
    public List<string>? StructuredTagsListResponse { get; }

    /// <summary>
    /// Поле для передачи идентификатора сохраненной/измененной заметки в обе стороны
    /// </summary>
    public int NoteIdExchange { get; }

    /// <summary>
    /// Сообщение об ошибке
    /// </summary>
    public string? CommonErrorMessageResponse { get; init; }


    /// <summary/> Создать незаполненный контейнер для заметки.
    public NoteResultDto() { }

    /// <summary/> Создать заполненный контейнер для заметки.
    public NoteResultDto(
        List<string> structuredTagsListResponse,
        int noteIdExchange = 0,
        string textResponse = "",
        string titleResponse = "",
        List<string>? tagsCheckedUncheckedResponse = null)
    {
        TextResponse = textResponse;
        TitleResponse = titleResponse;
        TagsCheckedUncheckedResponse = tagsCheckedUncheckedResponse ?? [];
        StructuredTagsListResponse = structuredTagsListResponse;
        NoteIdExchange = noteIdExchange;
    }
}
