using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SearchEngine.Data.Dto;
using SearchEngine.Data.Repository.Contracts;

namespace SearchEngine.Service.Models;

public class UpdateModel
{
    private const string GetOriginalNoteError = $"[{nameof(UpdateModel)}: {nameof(GetOriginalNote)} error]";
    private const string UpdateNoteError = $"[{nameof(UpdateModel)}: {nameof(UpdateNote)} error]";

    private readonly IDataRepository _repo;
    private readonly ILogger<UpdateModel> _logger;

    public UpdateModel(IServiceScope scope)
    {
        _repo = scope.ServiceProvider.GetRequiredService<IDataRepository>();
        _logger = scope.ServiceProvider.GetRequiredService<ILogger<UpdateModel>>();
    }

    public async Task<NoteDto> GetOriginalNote(int originalNoteId)
    {
        try
        {
            string text;
            var title = string.Empty;

            var notes = await _repo
                .ReadNote(originalNoteId)
                .ToListAsync();

            if (notes.Count > 0)// if exists что ли?
            {
                text = notes[0].Item1;

                title = notes[0].Item2;
            }
            else
            {
                text = "[Note: Always Deleted. Select another pls]";
            }

            var tagList = await _repo.ReadGeneralTagList();

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
