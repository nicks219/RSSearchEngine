using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RandomSongSearchEngine.Data;
using RandomSongSearchEngine.Data.Dto;
using RandomSongSearchEngine.Infrastructure.Cache.Contracts;
using RandomSongSearchEngine.Service.Models;

namespace RandomSongSearchEngine.Controllers;

[Route("api/update")]
[ApiController]
public class UpdateController : ControllerBase
{
    private readonly ILogger<UpdateController> _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public UpdateController(IServiceScopeFactory serviceScopeFactory, ILogger<UpdateController> logger, IHttpContextAccessor accessor)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
        var isAuthorized = accessor.HttpContext?.User.Claims.Any();
    }

    [HttpGet]
    public async Task<ActionResult<NoteDto>> GetInitialNote(int id)
    {
        try
        {
            using var scope = _serviceScopeFactory.CreateScope();
            return await new UpdateModel(scope).GetInitialNote(id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"[{nameof(UpdateController)}: {nameof(GetInitialNote)} error]");
            return new NoteDto { ErrorMessageResponse = $"[{nameof(UpdateController)}: {nameof(GetInitialNote)} error]" };
        }
    }

    [Authorize]
    [HttpPost]
    public async Task<ActionResult<NoteDto>> UpdateNote([FromBody] NoteDto dto)
    {
        try
        {
            using var scope = _serviceScopeFactory.CreateScope();
            
            var cache = scope.ServiceProvider.GetRequiredService<ICacheRepository>();
            cache.Update(dto.Id, new TextEntity{Title = dto.Title, Song = dto.Text});
            
            return await new UpdateModel(scope).UpdateNote(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"[{nameof(UpdateController)}: {nameof(UpdateNote)} error]");
            return new NoteDto { ErrorMessageResponse = $"[{nameof(UpdateController)}: {nameof(UpdateNote)} error]" };
        }
    }
}