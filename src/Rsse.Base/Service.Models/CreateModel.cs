using System.Text.RegularExpressions;
using RandomSongSearchEngine.Data.Dto;
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

    public async Task<NoteDto> ReadGeneralTagList()
    {
        try
        {
            var tagList = await _repo.ReadGeneralTagList();
            
            return new NoteDto(tagList);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"[{nameof(CreateModel)}: {nameof(ReadGeneralTagList)} error]");
            
            return new NoteDto() {ErrorMessageResponse = $"[{nameof(CreateModel)}: {nameof(ReadGeneralTagList)} error]"};
        }
    }
    
    public async Task<NoteDto> CreateNote(NoteDto createdNote)
    {
        try
        {
            if (createdNote.SongGenres == null || string.IsNullOrEmpty(createdNote.Text)
                                               || string.IsNullOrEmpty(createdNote.Title) ||
                                               createdNote.SongGenres.Count == 0)
            {
                var errorDto = await ReadGeneralTagList();
                
                errorDto.ErrorMessageResponse = $"[{nameof(CreateModel)}: {nameof(CreateNote)} error: empty data]";

                if (string.IsNullOrEmpty(createdNote.Text))
                {
                    return errorDto;
                }
                
                errorDto.TextResponse = createdNote.Text;
                errorDto.TitleResponse = createdNote.Title;

                return errorDto;
            }

            //createdNote.Text =  Encoding.Convert(Encoding.Default, Encoding.UTF8, Encoding.Default.GetBytes(createdNote.Text)).ToString();

            createdNote.Title = createdNote.Title.Trim();
            
            var newNoteId = await _repo.CreateNote(createdNote);
            
            if (newNoteId == 0)
            {
                var errorDto = await ReadGeneralTagList();
                
                errorDto.ErrorMessageResponse = $"[{nameof(CreateModel)}: {nameof(CreateNote)} error: create unsuccessful]";
                
                errorDto.TitleResponse = "[Already Exist]";
                
                return errorDto;
            }

            var updatedDto = await ReadGeneralTagList();
            
            var updatedTagList = updatedDto.GenreListResponse!;
            
            var checkboxes = new List<string>();
            
            for (var i = 0; i < updatedTagList.Count; i++)
            {
                checkboxes.Add("unchecked");
            }

            foreach (var i in createdNote.SongGenres)
            {
                checkboxes[i - 1] = "checked";
            }

            return new NoteDto(updatedTagList, newNoteId, "", "[OK]", checkboxes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"[{nameof(CreateModel)}: {nameof(CreateNote)} error]");
            
            return new NoteDto { ErrorMessageResponse = $"[{nameof(CreateModel)}: {nameof(CreateNote)} error]" };
        }
    }
    
    // \[([^\[\]]+)\]
    private static readonly Regex TitlePattern = new(@"\[(.+?)\]", RegexOptions.Compiled);
    
    public Task CreateTag(NoteDto? dto)
    {
        if (dto?.Title == null)
        {
            return Task.CompletedTask;
        }
        
        var tag = TitlePattern.Match(dto.Title).Value.Trim("[]".ToCharArray());

        return !string.IsNullOrEmpty(tag) ? _repo.CreateTagIfNotExists(tag) : Task.CompletedTask;
    }
}