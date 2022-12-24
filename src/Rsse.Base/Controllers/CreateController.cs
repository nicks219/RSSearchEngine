using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RandomSongSearchEngine.Data;
using RandomSongSearchEngine.Data.Dto;
using RandomSongSearchEngine.Infrastructure.Cache.Contracts;
using RandomSongSearchEngine.Service.Models;

namespace RandomSongSearchEngine.Controllers;

[Authorize]
[Route("api/create")]
[ApiController]
public class CreateController : ControllerBase
{
    private readonly ILogger<CreateController> _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public CreateController(IServiceScopeFactory serviceScopeFactory, ILogger<CreateController> logger)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<NoteDto>> OnGetGenreListAsync()
    {
        try
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var model = new CreateModel(scope);
            return await model.ReadGeneralTagList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"[{nameof(CreateController)}: {nameof(OnGetGenreListAsync)} error]");
            return new NoteDto { ErrorMessageResponse = $"[{nameof(CreateController)}: {nameof(OnGetGenreListAsync)} error]" };
        }
    }

    [HttpPost]
    public async Task<ActionResult<NoteDto>> CreateNoteAsync([FromBody] NoteDto dto)
    {
        
        try
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var model = new CreateModel(scope);

            var result =  await model.CreateNote(dto);

            if (string.IsNullOrEmpty(result.ErrorMessageResponse))
            {
                await model.CreateTag(dto); // [CREATE GENRE]
                
                var cache = scope.ServiceProvider.GetRequiredService<ICacheRepository>();
                
                cache.Create(result.Id, new TextEntity{Title = dto.Title, Song = dto.Text});
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"[{nameof(CreateController)}: {nameof(CreateNoteAsync)} error]");
            return new NoteDto { ErrorMessageResponse = $"[{nameof(CreateController)}: {nameof(CreateNoteAsync)} error]" };
        }
    }
}