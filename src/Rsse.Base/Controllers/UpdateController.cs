using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RandomSongSearchEngine.Data.DTO;
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
    public async Task<ActionResult<SongDto>> OnGetOriginalSongAsync(int id)
    {
        try
        {
            using var scope = _serviceScopeFactory.CreateScope();
            return await new UpdateModel(scope).ReadOriginalSongAsync(id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[UpdateController: OnGet Error]");
            return new SongDto() {ErrorMessageResponse = "[UpdateController: OnGet Error]"};
        }
    }

    [Authorize]
    [HttpPost]
    public async Task<ActionResult<SongDto>> UpdateSongAsync([FromBody] SongDto dto)
    {
        try
        {
            using var scope = _serviceScopeFactory.CreateScope();
            
            var cache = scope.ServiceProvider.GetRequiredService<ICacheRepository>();
            cache.Update(dto.Id, string.Concat(dto.Id, " '", dto.Title!, "' '", dto.Text!,"'"));
            
            return await new UpdateModel(scope).UpdateSongAsync(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[UpdateController: OnPost Error]");
            return new SongDto() {ErrorMessageResponse = "[UpdateController: OnPost Error]"};
        }
    }
}