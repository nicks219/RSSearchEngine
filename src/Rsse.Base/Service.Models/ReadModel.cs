using Microsoft.EntityFrameworkCore;
using RandomSongSearchEngine.Data.DTO;
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

    public string? ReadSongTitleById(int id)
    {
        try
        {
            var res = _repo.ReadSongTitleById(id);
            
            return res;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[ReadModel: ReadSongTitleById error]");
            
            return null;
        }
    }

    public async Task<SongDto> ReadGenreListAsync()
    {
        try
        {
            var genreListResponse = await _repo.ReadGenreListAsync();
            
            return new SongDto(genreListResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[ReadModel: OnGet Error]");
            
            return new SongDto() {ErrorMessageResponse = "[ReadModel: OnGet Error]"};
        }
    }

    public async Task<SongDto> ReadRandomSongAsync(SongDto? request, string? id = null)
    {
        var textResponse = "";
        
        var titleResponse = "";
        
        var songId = 0;
        
        try
        {
            if (request is {SongGenres: { }} && request.SongGenres.Count != 0)
            {
                if (!int.TryParse(id, out songId))
                {
                    songId = await _repo.GetBalancedIdAsync(request.SongGenres);
                }

                if (songId != 0)
                {
                    var song = await _repo
                        .ReadSong(songId)
                        .ToListAsync();
                    
                    if (song.Count > 0)
                    {
                        textResponse = song[0].Item1;
                        
                        titleResponse = song[0].Item2;
                    }
                }
            }

            var genreListResponse = await _repo.ReadGenreListAsync();
            
            return new SongDto(genreListResponse, songId, textResponse, titleResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[ReadModel: OnPost Error]");
            
            return new SongDto() {ErrorMessageResponse = "[ReadModel: OnPost Error]"};
        }
    }
}