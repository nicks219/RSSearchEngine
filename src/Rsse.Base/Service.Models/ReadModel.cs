using Microsoft.EntityFrameworkCore;
using RandomSongSearchEngine.Data.DTO;
using RandomSongSearchEngine.Data.Repository.Contracts;
using RandomSongSearchEngine.Infrastructure.Engine;

namespace RandomSongSearchEngine.Service.Models;

public class ReadModel
{
    private readonly IServiceScope _scope;
    
    private readonly ILogger<ReadModel> _logger;

    public ReadModel(IServiceScope serviceScope)
    {
        _scope = serviceScope;
        
        _logger = _scope.ServiceProvider.GetRequiredService<ILogger<ReadModel>>();
    }

    public string? ReadSongTitleById(int id)
    {
        using var repo = _scope.ServiceProvider.GetRequiredService<IDataRepository>();

        try
        {
            var res = repo.ReadSongTitleById(id);
            
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
        await using var repo = _scope.ServiceProvider.GetRequiredService<IDataRepository>();
        
        try
        {
            var genreListResponse = await repo.ReadGenreListAsync();
            
            return new SongDto(genreListResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[ReadModel: OnGet Error]");
            
            return new SongDto() {ErrorMessageResponse = "[ReadModel: OnGet Error]"};
        }
    }

    public async Task<SongDto> ReadRandomSongAsync(SongDto? request)
    {
        var textResponse = "";
        
        var titleResponse = "";
        
        var songId = 0;
        
        await using var repo = _scope.ServiceProvider.GetRequiredService<IDataRepository>();
        
        try
        {
            if (request is {SongGenres: { }} && request.SongGenres.Count != 0)
            {
                songId = await repo.ReadRandomIdAsync(request.SongGenres);
                
                if (songId != 0)
                {
                    var song = await repo
                        .ReadSong(songId)
                        .ToListAsync();
                    
                    if (song.Count > 0)
                    {
                        textResponse = song[0].Item1;
                        
                        titleResponse = song[0].Item2;
                    }
                }
            }

            var genreListResponse = await repo.ReadGenreListAsync();
            
            return new SongDto(genreListResponse, songId, textResponse, titleResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[ReadModel: OnPost Error]");
            
            return new SongDto() {ErrorMessageResponse = "[ReadModel: OnPost Error]"};
        }
    }
}