using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SearchEngine.Data;
using SearchEngine.Data.Dto;
using SearchEngine.Infrastructure.Cache.Contracts;
using SearchEngine.Service.Models;

namespace SearchEngine.Controllers;

[Route("api/update")]
[ApiController]
public class UpdateController : ControllerBase
{
    private const string GetInitialNoteError = $"[{nameof(UpdateController)}: {nameof(GetInitialNote)} error]";
    private const string UpdateNoteError = $"[{nameof(UpdateController)}: {nameof(UpdateNote)} error]";

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
            return await new UpdateModel(scope).GetOriginalNote(id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, GetInitialNoteError);
            return new NoteDto { ErrorMessageResponse = GetInitialNoteError };
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
            cache.Update(dto.Id, new TextEntity { Title = dto.Title, Song = dto.Text });

            return await new UpdateModel(scope).UpdateNote(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, UpdateNoteError);
            return new NoteDto { ErrorMessageResponse = UpdateNoteError };
        }
    }
}
