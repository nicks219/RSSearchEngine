using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SearchEngine.Domain.Contracts;
using SearchEngine.Domain.Dto;
using static SearchEngine.Domain.Configuration.ErrorMessages;

namespace SearchEngine.Domain.Managers;

/// <summary>
/// Функционал обновления заметок
/// </summary>
public class UpdateManager(IDataRepository repo, ILogger logger)
{
    /// <summary>
    /// Прочитать обновляемую заметку
    /// </summary>
    /// <param name="originalNoteId">идентификатор обновляемой заметки</param>
    /// <returns>ответ с заметкой</returns>
    public async Task<NoteResultDto> GetOriginalNote(int originalNoteId)
    {
        try
        {
            string text;
            var title = string.Empty;

            var notes = await repo
                .ReadNote(originalNoteId)
                .ToListAsync();

            if (notes.Count > 0)
            {
                // сначала текст потом название
                text = notes[0].Text;

                title = notes[0].Title;
            }
            else
            {
                text = $"[{nameof(GetOriginalNote)}] action is not possible, note to be updated is not specified";
            }

            var tagList = await repo.ReadStructuredTagList();

            var noteTags = await repo
                .ReadNoteTags(originalNoteId)
                .ToListAsync();

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
            return new NoteResultDto { CommonErrorMessageResponse = GetOriginalNoteError };
        }
    }

    /// <summary>
    /// Обновить заметку
    /// </summary>
    /// <param name="updatedNoteRequest">данные для обновления</param>
    /// <returns>обновленная заметка</returns>
    public async Task<NoteResultDto> UpdateNote(NoteRequestDto updatedNoteRequest)
    {
        try
        {
            if (updatedNoteRequest.TagsCheckedRequest == null
                || string.IsNullOrEmpty(updatedNoteRequest.TextRequest)
                || string.IsNullOrEmpty(updatedNoteRequest.TitleRequest)
                || updatedNoteRequest.TagsCheckedRequest.Count == 0)
            {
                return await GetOriginalNote(updatedNoteRequest.NoteIdExchange);
            }

            var initialNoteTags = await repo
                .ReadNoteTags(updatedNoteRequest.NoteIdExchange)
                .ToListAsync();

            await repo.UpdateNote(initialNoteTags, updatedNoteRequest);

            return await GetOriginalNote(updatedNoteRequest.NoteIdExchange);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, UpdateNoteError);

            return new NoteResultDto { CommonErrorMessageResponse = UpdateNoteError };
        }
    }
}
