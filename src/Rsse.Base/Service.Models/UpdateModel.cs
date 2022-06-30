using Microsoft.EntityFrameworkCore;
using RandomSongSearchEngine.Data.DTO;
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

    public async Task<SongDto> ReadOriginalSongAsync(int originalSongId)
    {
        try
        {
            string textResponse;

            var titleResponse = "";

            var song = await _repo
                .ReadSong(originalSongId)
                .ToListAsync();

            if (song.Count > 0)
            {
                textResponse = song[0].Item1;

                titleResponse = song[0].Item2;
            }
            else
            {
                textResponse = "[Song: Always Deleted. Select another pls]";
            }

            var genreListResponse = await _repo.ReadGenreListAsync();

            var songGenres = await _repo
                .ReadSongGenres(originalSongId)
                .ToListAsync();

            var songGenresResponse = new List<string>();

            for (var i = 0; i < genreListResponse.Count; i++)
            {
                songGenresResponse.Add("unchecked");
            }

            foreach (var i in songGenres)
            {
                songGenresResponse[i - 1] = "checked";
            }

            return new SongDto(genreListResponse, originalSongId, textResponse, titleResponse, songGenresResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[ChangeTextModel: OnGet Error]");

            return new SongDto() {ErrorMessageResponse = "[ChangeTextModel: OnGet Error]"};
        }
    }

    public async Task<SongDto> UpdateSongAsync(SongDto updatedSong)
    {
        try
        {
            if (updatedSong.SongGenres == null
                || string.IsNullOrEmpty(updatedSong.Text)
                || string.IsNullOrEmpty(updatedSong.Title)
                || updatedSong.SongGenres.Count == 0)
            {
                return await ReadOriginalSongAsync(updatedSong.Id);
            }

            var originalGenres = await _repo
                .ReadSongGenres(updatedSong.Id)
                .ToListAsync();

            await _repo.UpdateSongAsync(originalGenres, updatedSong);

            return await ReadOriginalSongAsync(updatedSong.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[ChangeTextModel: OnPost Error]");

            return new SongDto() {ErrorMessageResponse = "[ChangeTextModel: OnPost Error]"};
        }
    }
}
