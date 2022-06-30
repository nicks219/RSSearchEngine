using System.Text.RegularExpressions;
using RandomSongSearchEngine.Data.DTO;
using RandomSongSearchEngine.Data.Repository.Contracts;

namespace RandomSongSearchEngine.Service.Models;

public class CreateModel
{
    private readonly IDataRepository _repo;
    private readonly ILogger<CreateModel> _logger;

    public CreateModel(IServiceScope serviceScope)
    {
        _repo = serviceScope.ServiceProvider.GetRequiredService<IDataRepository>();
        
        _logger = serviceScope.ServiceProvider.GetRequiredService<ILogger<CreateModel>>();
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
            _logger.LogError(ex, "[CreateModel: OnGet Error]");
            
            return new SongDto() {ErrorMessageResponse = "[CreateModel: OnGet Error]"};
        }
    }
    
    public async Task<SongDto> CreateSongAsync(SongDto createdSong)
    {
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

            //createdSong.Text =  Encoding.Convert(Encoding.Default, Encoding.UTF8, Encoding.Default.GetBytes(createdSong.Text)).ToString();

            createdSong.Title = createdSong.Title.Trim();
            
            var newSongId = await _repo.CreateSongAsync(createdSong);
            
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
    
    // \[([^\[\]]+)\]
    private static readonly Regex TitlePattern = new(@"\[(.+?)\]", RegexOptions.Compiled);
    
    public Task CreateGenreAsync(SongDto? dto)
    {
        if (dto?.Title == null)
        {
            return Task.CompletedTask;
        }
        
        var tag = TitlePattern.Match(dto.Title).Value.Trim("[]".ToCharArray());

        return !string.IsNullOrEmpty(tag) ? _repo.CreateGenreIfNotExistsAsync(tag) : Task.CompletedTask;
    }
}