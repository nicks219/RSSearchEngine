using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SearchEngine.Data.Dto;
using SearchEngine.Data.Repository.Contracts;
using static SearchEngine.Common.ModelMessages;

namespace SearchEngine.Models;

/// <summary>
/// Функционал обновления заметок
/// </summary>
public class UpdateModel(IServiceScope scope)
{
    private readonly IDataRepository _repo = scope.ServiceProvider.GetRequiredService<IDataRepository>();
    private readonly ILogger<UpdateModel> _logger = scope.ServiceProvider.GetRequiredService<ILogger<UpdateModel>>();

    /// <summary>
    /// Прочитать обновляемую заметку
    /// </summary>
    /// <param name="originalNoteId">идентификатор обновляемой заметки</param>
    /// <returns>ответ с заметкой</returns>
    public async Task<NoteDto> GetOriginalNote(int originalNoteId)
    {
        try
        {
            string text;
            var title = string.Empty;

            var notes = await _repo
                .ReadNote(originalNoteId)
                .ToListAsync();

            if (notes.Count > 0)
            {
                text = notes[0].Item1;

                title = notes[0].Item2;
            }
            else
            {
                text = $"[{nameof(GetOriginalNote)}] action is not possible, note to be updated is not specified";
            }

            var tagList = await _repo.ReadStructuredTagList();

            var noteTags = await _repo
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

            return new NoteDto(tagList, originalNoteId, text, title, checkboxes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, GetOriginalNoteError);
            return new NoteDto { CommonErrorMessageResponse = GetOriginalNoteError };
        }
    }

    /// <summary>
    /// Обновить заметку
    /// </summary>
    /// <param name="updatedNote">данные для обновления</param>
    /// <returns>обновленная заметка</returns>
    public async Task<NoteDto> UpdateNote(NoteDto updatedNote)
    {
        try
        {
            if (updatedNote.TagsCheckedRequest == null
                || string.IsNullOrEmpty(updatedNote.TextRequest)
                || string.IsNullOrEmpty(updatedNote.TitleRequest)
                || updatedNote.TagsCheckedRequest.Count == 0)
            {
                return await GetOriginalNote(updatedNote.CommonNoteId);
            }

            var initialNoteTags = await _repo
                .ReadNoteTags(updatedNote.CommonNoteId)
                .ToListAsync();

            await _repo.UpdateNote(initialNoteTags, updatedNote);

            return await GetOriginalNote(updatedNote.CommonNoteId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, UpdateNoteError);

            return new NoteDto { CommonErrorMessageResponse = UpdateNoteError };
        }
    }
}
