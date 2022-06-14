using RandomSongSearchEngine.Data.DTO;
using RandomSongSearchEngine.Data.Repository.Contracts;

namespace RandomSongSearchEngine.Service.Models;

public class CreateModel
{
    private readonly IServiceScope _scope;
    private readonly ILogger<CreateModel> _logger;

    public CreateModel(IServiceScope serviceScope)
    {
        _scope = serviceScope;
        
        _logger = _scope.ServiceProvider.GetRequiredService<ILogger<CreateModel>>();
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
            _logger.LogError(ex, "[CreateModel: OnGet Error]");
            
            return new SongDto() {ErrorMessageResponse = "[CreateModel: OnGet Error]"};
        }
    }
    
    public async Task<SongDto> CreateSongAsync(SongDto createdSong)
    {
        await using var repo = _scope.ServiceProvider.GetRequiredService<IDataRepository>();
        
        try
        {
            if (createdSong.SongGenres == null || string.IsNullOrEmpty(createdSong.Text)
                                               || string.IsNullOrEmpty(createdSong.Title) ||
                                               createdSong.SongGenres.Count == 0)
            {
                var errorDto = await ReadGenreListAsync();
                
                errorDto.ErrorMessageResponse = "[CreateModel: OnPost Error - empty data]";

                if (!string.IsNullOrEmpty(createdSong.Text))
                {
                    errorDto.TextResponse = createdSong.Text;
                    errorDto.TitleResponse = createdSong.Title;
                }
                
                return errorDto;
            }

            createdSong.Title = createdSong.Title.Trim();
            
            var newSongId = await repo.CreateSongAsync(createdSong);
            
            if (newSongId == 0)
            {
                var errorDto = await ReadGenreListAsync();
                
                errorDto.ErrorMessageResponse = "[CreateModel: OnPost Error - create unsuccessful]";
                
                errorDto.TitleResponse = "[Already Exist]";
                
                return errorDto;
            }

            var updatedDto = await ReadGenreListAsync();
            
            var updatedGenreList = updatedDto.GenreListResponse!;
            
            var songGenresResponse = new List<string>();
            
            for (var i = 0; i < updatedGenreList.Count; i++)
            {
                songGenresResponse.Add("unchecked");
            }

            foreach (var i in createdSong.SongGenres)
            {
                songGenresResponse[i - 1] = "checked";
            }

            return new SongDto(updatedGenreList, newSongId, "", "[OK]", songGenresResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[CreateModel: OnPost Error]");
            
            return new SongDto() {ErrorMessageResponse = "[CreateModel: OnPost Error]"};
        }
    }
}