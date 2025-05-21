using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SearchEngine.Domain.Contracts;
using SearchEngine.Domain.Dto;
using static SearchEngine.Domain.Configuration.ServiceErrorMessages;

namespace SearchEngine.Domain.Services;

/// <summary>
/// Функционал обновления заметок
/// </summary>
public class UpdateService(IDataRepository repo, ILogger<UpdateService> logger)
{
    /// <summary>
    /// Обновить заметку
    /// </summary>
    /// <param name="updatedNoteRequest">данные для обновления</param>
    /// <returns>обновленная заметка</returns>
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
    /// Прочитать обновляемую заметку
    /// </summary>
    /// <param name="originalNoteId">идентификатор обновляемой заметки</param>
    /// <returns>ответ с заметкой</returns>
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
