using System.Collections.Generic;

namespace SearchEngine.Data.Dto;

/// <summary>
/// Контейнер для ответа с заметкой.
/// </summary>
public record NoteResultDto
{
    /// <summary>
    /// Представление списка тегов в ответе в виде флагов:<br/><b>true</b> - отмечен | <b>false</b> - не отмечен.
    /// </summary>
    public List<bool>? CheckedUncheckedTags { get; }

    /// <summary>
    /// Именование заметки в ответе.
    /// </summary>
    public string? Title { get; init; }

    /// <summary>
    /// Текст заметки в ответе.
    /// </summary>
    public string? Text { get; init; }

    /// <summary>
    /// Список тегов в формате "имя : количество записей по тегу".
    /// </summary>
    public List<string>? EnrichedTags { get; }

    /// <summary>
    /// Поле для передачи идентификатора сохраненной/измененной заметки в обе стороны.
    /// </summary>
    public int NoteIdExchange { get; }

    /// <summary>
    /// Сообщение об ошибке.
    /// </summary>
    public string? ErrorMessage { get; init; }


    /// <summary>
    /// Создать незаполненный контейнер для заметки.
    /// </summary>
    public NoteResultDto() { }

    /// <summary/> Создать заполненный контейнер с заметкой.
    public NoteResultDto(
        List<string> enrichedTags,
        int noteIdExchange = 0,
        string text = "",
        string title = "",
        List<bool>? checkedUncheckedTags = null)
    {
        Text = text;
        Title = title;
        CheckedUncheckedTags = checkedUncheckedTags ?? [];
        EnrichedTags = enrichedTags;
        NoteIdExchange = noteIdExchange;
    }
}
