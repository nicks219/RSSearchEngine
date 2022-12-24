using Microsoft.EntityFrameworkCore;
using RandomSongSearchEngine.Data.Dto;
using RandomSongSearchEngine.Data.Repository.Contracts;

namespace RandomSongSearchEngine.Service.Models;

public class UpdateModel
{
    private readonly IDataRepository _repo;
    
    private readonly ILogger<UpdateModel> _logger;

    public UpdateModel(IServiceScope scope)
    {
        _repo = scope.ServiceProvider.GetRequiredService<IDataRepository>();

        _logger = scope.ServiceProvider.GetRequiredService<ILogger<UpdateModel>>();
    }

    public async Task<NoteDto> GetInitialNote(int initialNoteId)
    {
        try
        {
            string text;

            var title = "";

            var notes = await _repo
                .ReadNote(initialNoteId)
                .ToListAsync();

            if (notes.Count > 0)
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
                .ReadNoteTags(initialNoteId)
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

            return new NoteDto(tagList, initialNoteId, text, title, checkboxes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"[{nameof(UpdateModel)}: {nameof(GetInitialNote)} error]");

            return new NoteDto { ErrorMessageResponse = $"[{nameof(UpdateModel)}: {nameof(GetInitialNote)} error]" };
        }
    }

    public async Task<NoteDto> UpdateNote(NoteDto updatedNote)
    {
        try
        {
            if (updatedNote.SongGenres == null
                || string.IsNullOrEmpty(updatedNote.Text)
                || string.IsNullOrEmpty(updatedNote.Title)
                || updatedNote.SongGenres.Count == 0)
            {
                return await GetInitialNote(updatedNote.Id);
            }

            var initialNoteTags = await _repo
                .ReadNoteTags(updatedNote.Id)
                .ToListAsync();

            await _repo.UpdateNote(initialNoteTags, updatedNote);

            return await GetInitialNote(updatedNote.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"[{nameof(UpdateModel)}: {nameof(UpdateNote)} error]");

            return new NoteDto { ErrorMessageResponse = $"[{nameof(UpdateModel)}: {nameof(UpdateNote)} error]" };
        }
    }
}
