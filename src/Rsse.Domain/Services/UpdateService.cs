using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SearchEngine.Data.Contracts;
using SearchEngine.Data.Dto;
using static SearchEngine.Service.Configuration.ServiceErrorMessages;

namespace SearchEngine.Services;

/// <summary>
/// Функционал обновления заметок.
/// </summary>
public class UpdateService(IDataRepository repo, ILogger<UpdateService> logger)
{
    /// <summary>
    /// Обновить заметку.
    /// </summary>
    /// <param name="updatedNoteRequest">Данные для обновления.</param>
    /// <returns>Обновленная заметка.</returns>
    public async Task<NoteResultDto> UpdateNote(NoteRequestDto updatedNoteRequest)
    {
        try
        {
            if (updatedNoteRequest.CheckedTags == null
                || string.IsNullOrEmpty(updatedNoteRequest.Text)
                || string.IsNullOrEmpty(updatedNoteRequest.Title)
                || updatedNoteRequest.CheckedTags.Count == 0)
            {
                return await GetNoteWithTagsForUpdate(updatedNoteRequest.NoteIdExchange);
            }

            var initialNoteTags = await repo
                .ReadNoteTagIds(updatedNoteRequest.NoteIdExchange);

            await repo.UpdateNote(initialNoteTags, updatedNoteRequest);

            return await GetNoteWithTagsForUpdate(updatedNoteRequest.NoteIdExchange);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, UpdateNoteError);

            return new NoteResultDto { ErrorMessage = UpdateNoteError };
        }
    }

    /// <summary>
    /// Прочитать обновляемую заметку.
    /// </summary>
    /// <param name="originalNoteId">Идентификатор обновляемой заметки.</param>
    /// <returns>Ответ с заметкой.</returns>
    public async Task<NoteResultDto> GetNoteWithTagsForUpdate(int originalNoteId)
    {
        try
        {
            string text;
            var title = string.Empty;

            var note = await repo.ReadNote(originalNoteId);

            if (note != null)
            {
                text = note.Text;

                title = note.Title;
            }
            else
            {
                text = $"[{nameof(GetNoteWithTagsForUpdate)}] action is not possible, note to be updated is not specified";
            }

            var tagList = await repo.ReadEnrichedTagList();

            var noteTags = await repo.ReadNoteTagIds(originalNoteId);

            var checkboxes = new List<string>();

            for (var i = 0; i < tagList.Count; i++)
            {
                checkboxes.Add("unchecked");
            }

            foreach (var i in noteTags)
            {
                checkboxes[i - 1] = "checked";
            }

            return new NoteResultDto(tagList, originalNoteId, text, title, checkboxes);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, GetOriginalNoteError);
            return new NoteResultDto { ErrorMessage = GetOriginalNoteError };
        }
    }
}
