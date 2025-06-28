using System.Collections.Generic;
using System.Linq;
using Rsse.Domain.Data.Dto;

namespace Rsse.Domain.Data.Common;

/// <summary>
/// Компонент, инкапсулирующий логику создания dto.
/// </summary>
public static class NoteResult
{
    /// <summary>
    /// Создать dto с ответом, представляющим заметку.
    /// </summary>
    /// <param name="markedTags">Все имеющиеся теги, с флагом соответствия.</param>
    /// <param name="noteId">Идентификатор заметки.</param>
    /// <param name="text">Текст заметки.</param>
    /// <param name="title">Именование заметки.</param>
    /// <returns></returns>
    public static NoteResultDto CreateFrom(List<TagMarkedResultDto> markedTags, int noteId, string text, string title)
    {
        var checkedUncheckedTags = markedTags.Select(t => t.IsChecked).ToList();
        var enrichedTags = markedTags.Select(t => t.GetEnrichedName()).ToList();
        var noteResultDto = new NoteResultDto(enrichedTags, noteId, text, title, checkedUncheckedTags);

        return noteResultDto;
    }
}
