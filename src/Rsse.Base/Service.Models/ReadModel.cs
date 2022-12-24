using Microsoft.EntityFrameworkCore;
using RandomSongSearchEngine.Data.Dto;
using RandomSongSearchEngine.Data.Repository.Contracts;
using RandomSongSearchEngine.Infrastructure.Engine;

namespace RandomSongSearchEngine.Service.Models;

public class ReadModel
{
    private readonly ILogger<ReadModel> _logger;

    private readonly IDataRepository _repo;

    public ReadModel(IServiceScope serviceScope)
    {
        _logger = serviceScope.ServiceProvider.GetRequiredService<ILogger<ReadModel>>();
        
        _repo = serviceScope.ServiceProvider.GetRequiredService<IDataRepository>();
    }

    public string? ReadTitleByNoteId(int id)
    {
        try
        {
            var res = _repo.ReadTitleByNoteId(id);
            
            return res;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"[{nameof(ReadModel)}: {nameof(ReadTitleByNoteId)} error]");
            
            return null;
        }
    }

    public async Task<NoteDto> ReadTagList()
    {
        try
        {
            var tagList = await _repo.ReadGeneralTagList();
            
            return new NoteDto(tagList);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"[{nameof(ReadModel)}: {nameof(ReadTagList)} error]");
            
            return new NoteDto {ErrorMessageResponse = $"[{nameof(ReadModel)}: {nameof(ReadTagList)} error]"};
        }
    }

    public async Task<NoteDto> ElectNote(NoteDto? request, string? id = null, bool randomElection = true)
    {
        var text = "";
        
        var title = "";
        
        var noteId = 0;
        
        try
        {
            if (request is {SongGenres: { }} && request.SongGenres.Count != 0)
            {
                if (!int.TryParse(id, out noteId))
                {
                    noteId = await _repo.ElectNoteId(request.SongGenres, randomElection);
                }

                if (noteId != 0)
                {
                    var song = await _repo
                        .ReadNote(noteId)
                        .ToListAsync();
                    
                    if (song.Count > 0)
                    {
                        text = song[0].Item1;
                        
                        title = song[0].Item2;
                    }
                }
            }

            var genreListResponse = await _repo.ReadGeneralTagList();
            
            return new NoteDto(genreListResponse, noteId, text, title);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"[{nameof(ReadModel)}: {nameof(ElectNote)} error]");
            
            return new NoteDto {ErrorMessageResponse = $"[{nameof(ReadModel)}: {nameof(ElectNote)} error]"};
        }
    }
}