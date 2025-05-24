
using System.Threading;
using System.Threading.Tasks;
using SearchEngine.Data.Contracts;
using SearchEngine.Data.Dto;

namespace SearchEngine.Services;

/// <summary>
/// Функционал обновления заметок.
/// </summary>
public class UpdateService(IDataRepository repo)
{
    /// <summary>
    /// Обновить заметку.
    /// </summary>
    /// <param name="updatedNoteRequest">Данные для обновления.</param>
    /// <param name="ct">Токен отмены.</param>
    /// <returns>Обновленная заметка.</returns>
    public async Task<NoteResultDto> UpdateNote(NoteRequestDto updatedNoteRequest, CancellationToken ct)
    {
        if (updatedNoteRequest.CheckedTags == null
            || string.IsNullOrEmpty(updatedNoteRequest.Text)
            || string.IsNullOrEmpty(updatedNoteRequest.Title)
            || updatedNoteRequest.CheckedTags.Count == 0)
        {
            return await GetNoteWithTagsForUpdate(updatedNoteRequest.NoteIdExchange, ct);
        }

        var initialNoteTags = await repo
            .ReadNoteTagIds(updatedNoteRequest.NoteIdExchange, ct);

        await repo.UpdateNote(initialNoteTags, updatedNoteRequest, ct);

        return await GetNoteWithTagsForUpdate(updatedNoteRequest.NoteIdExchange, ct);
    }

    /// <summary>
    /// Прочитать обновляемую заметку.
    /// </summary>
    /// <param name="originalNoteId">Идентификатор обновляемой заметки.</param>
    /// <param name="ct">Токен отмены.</param>
    /// <returns>Ответ с заметкой.</returns>
    public async Task<NoteResultDto> GetNoteWithTagsForUpdate(int originalNoteId, CancellationToken ct)
    {
        string text;
        var title = string.Empty;

        var note = await repo.ReadNote(originalNoteId, ct);

        if (note != null)
        {
            text = note.Text;

            title = note.Title;
        }
        else
        {
            text = $"[{nameof(GetNoteWithTagsForUpdate)}] action is not possible, note to be updated is not specified";
        }

        var tagsBeforeUpdate = await repo.ReadEnrichedTagList(ct);
        var totalTagsCount = tagsBeforeUpdate.Count;
        var noteTagIds = await repo.ReadNoteTagIds(originalNoteId, ct);

        var checkboxes = TagConverter.AllToFlags(noteTagIds, totalTagsCount);

        return new NoteResultDto(tagsBeforeUpdate, originalNoteId, text, title, checkboxes);
    }
}
