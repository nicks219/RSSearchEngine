using System.Collections.Generic;

namespace SearchEngine.Domain.Dto;

/// <summary>
/// Шаблон передачи данных для заметки
/// </summary>
public record NoteRequestDto
(
    // Список отмеченных тегов в запросе
    List<int>? TagsCheckedRequest,
    // Именование заметки в запросе
    string? TitleRequest,
    // Текст заметки в запросе
    string? TextRequest,
    // Поле для передачи идентификатора сохраненной/измененной заметки в обе стороны
    int NoteIdExchange
);
