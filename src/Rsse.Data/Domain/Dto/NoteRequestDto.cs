using System.Collections.Generic;

namespace SearchEngine.Domain.Dto;

/// <summary>
/// Контейнер для запроса с заметкой.
/// </summary>
/// <param name="CheckedTags">Список отмеченных тегов.</param>
/// <param name="Title">Именование заметки.</param>
/// <param name="Text">Текст заметки.</param>
/// <param name="NoteIdExchange">Идентификатор сохраненной/измененной заметки, в обе стороны.</param>
public record NoteRequestDto
(
    List<int>? CheckedTags,
    string? Title,
    string? Text,
    int NoteIdExchange
);
