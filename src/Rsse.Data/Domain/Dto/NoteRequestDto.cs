using System.Collections.Generic;

namespace SearchEngine.Domain.Dto;

/// <summary>
/// Контейнер для запроса с заметкой.
/// </summary>
/// <param name="TagsCheckedRequest">Список отмеченных тегов.</param>
/// <param name="TitleRequest">Именование заметки.</param>
/// <param name="TextRequest">Текст заметки.</param>
/// <param name="NoteIdExchange">Идентификатор сохраненной/измененной заметки, в обе стороны.</param>
public record NoteRequestDto
(
    List<int>? TagsCheckedRequest,
    string? TitleRequest,
    string? TextRequest,
    int NoteIdExchange
);
