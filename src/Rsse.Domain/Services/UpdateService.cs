using System.Threading;
using System.Threading.Tasks;
using SearchEngine.Data.Common;
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
    /// <param name="stoppingToken">Токен отмены.</param>
    /// <returns>Обновленная заметка.</returns>
    public async Task<NoteResultDto> UpdateNote(NoteRequestDto updatedNoteRequest, CancellationToken stoppingToken)
    {
        if (updatedNoteRequest.CheckedTags == null
            || string.IsNullOrEmpty(updatedNoteRequest.Text)
            || string.IsNullOrEmpty(updatedNoteRequest.Title)
            || updatedNoteRequest.CheckedTags.Count == 0)
        {
            return await GetNoteWithTagsForUpdate(updatedNoteRequest.NoteIdExchange, stoppingToken);
        }

        await repo.UpdateNote(updatedNoteRequest, stoppingToken);

        return await GetNoteWithTagsForUpdate(updatedNoteRequest.NoteIdExchange, stoppingToken);
    }

    /// <summary>
    /// Прочитать обновляемую заметку.
    /// </summary>
    /// <param name="originalNoteId">Идентификатор обновляемой заметки.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Ответ с заметкой.</returns>
    public async Task<NoteResultDto> GetNoteWithTagsForUpdate(int originalNoteId, CancellationToken cancellationToken)
    {
        string text;
        var title = string.Empty;

        var note = await repo.ReadNote(originalNoteId, cancellationToken);

        if (note != null)
        {
            text = note.Text;

            title = note.Title;
        }
        else
        {
            text = $"[{nameof(GetNoteWithTagsForUpdate)}] action is not possible, note to be updated is not specified";
        }

        var tagsBeforeUpdate = await repo.ReadMarkedTags(originalNoteId, cancellationToken);
        var noteResultDto = NoteResult.CreateFrom(tagsBeforeUpdate, originalNoteId, text, title);

        return noteResultDto;
    }
}
