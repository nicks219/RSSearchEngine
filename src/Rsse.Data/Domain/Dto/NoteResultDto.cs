using System.Collections.Generic;

namespace SearchEngine.Domain.Dto;

/// <summary>
/// Контейнер с заметкой для ответа.
/// </summary>
public record NoteResultDto : NoteBaseResultDto
{
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
    /// Поле для передачи идентификатора сохраненной/измененной заметки в обе стороны
    /// </summary>
    public int NoteIdExchange { get; set; }

    /// <summary>
    /// Сообщение об ошибке
    /// </summary>
    public string? CommonErrorMessageResponse { get; init; }


    /// <summary>
    /// Создать незаполненный шаблон передачи данных для заметки
    /// </summary>
    public NoteResultDto() { }

    /// <summary>
    /// Создать шаблон передачи данных для заметки
    /// </summary>
    // todo: это response
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

/// <summary>
/// Контейнер с заметкой в случае ошибочного ответа.
/// </summary>
public record NoteErrorResultDto : NoteBaseResultDto
{
    /// <summary/> Именование заметки в ответе
    public string? TitleResponse { get; init; }
    /// <summary/> Текст заметки в ответе
    public string? TextResponse { get; init; }
    /// <summary/> Список тегов в формате "имя : количество записей"
    public List<string>? StructuredTagsListResponse { get; init; }
    /// <summary/> Сообщение об ошибке
    public string? CommonErrorMessageResponse { get; init; }
}

/// <summary>
/// Маркер контейнера с заметкой для ответа.
/// </summary>
public record NoteBaseResultDto;
