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
    public List<string>? CheckedUncheckedTags { get; }

    /// <summary>
    /// Именование заметки в ответе
    /// </summary>
    public string? Title { get; init; }

    /// <summary>
    /// Текст заметки в ответе
    /// </summary>
    public string? Text { get; init; }

    /// <summary>
    /// Список тегов в формате "имя : количество записей"
    /// </summary>
    public List<string>? StructuredTags { get; }

    /// <summary>
    /// Поле для передачи идентификатора сохраненной/измененной заметки в обе стороны
    /// </summary>
    public int NoteIdExchange { get; }

    /// <summary>
    /// Сообщение об ошибке
    /// </summary>
    public string? ErrorMessage { get; init; }


    /// <summary/> Создать незаполненный контейнер для заметки.
    public NoteResultDto() { }

    /// <summary/> Создать заполненный контейнер для заметки.
    public NoteResultDto(
        List<string> structuredTags,
        int noteIdExchange = 0,
        string text = "",
        string title = "",
        List<string>? tagsCheckedUncheckedResponse = null)
    {
        Text = text;
        Title = title;
        CheckedUncheckedTags = tagsCheckedUncheckedResponse ?? [];
        StructuredTags = structuredTags;
        NoteIdExchange = noteIdExchange;
    }
}
