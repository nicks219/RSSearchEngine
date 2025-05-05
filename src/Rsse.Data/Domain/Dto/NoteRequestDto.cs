using System.Collections.Generic;

namespace SearchEngine.Domain.Dto;

/// <summary>
/// Шаблон передачи данных для заметки
/// </summary>
public record NoteRequestDto
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
    // ReSharper disable once PropertyCanBeMadeInitOnly.Global
    public string? TextRequest { get; set; }

    /// <summary>
    /// Поле для передачи идентификатора сохраненной/измененной заметки в обе стороны
    /// </summary>
    public int NoteIdExchange { get; set; }
}
