using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SearchEngine.Data.Dto;
using SearchEngine.Data.Entities;
using SearchEngine.Engine.Contracts;
using SearchEngine.Models;

namespace SearchEngine.Controllers;

/// <summary>
/// Контроллер для обновления заметки
/// </summary>

[Route("api/update"), ApiController]

public class UpdateController : ControllerBase
{
    private const string GetInitialNoteError = $"[{nameof(UpdateController)}] {nameof(GetInitialNote)} error";
    private const string UpdateNoteError = $"[{nameof(UpdateController)}] {nameof(UpdateNote)} error";

    private readonly ILogger<UpdateController> _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public UpdateController(IServiceScopeFactory serviceScopeFactory, ILogger<UpdateController> logger, IHttpContextAccessor accessor)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
    }

    /// <summary>
    /// Получить обновляемую заметку
    /// </summary>
    /// <param name="id">идентификатор обновляемой заметки</param>
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
            return new NoteDto { CommonErrorMessageResponse = GetInitialNoteError };
        }
    }

    /// <summary>
    /// Обновить заметку
    /// </summary>
    /// <param name="dto">данные для обновления</param>
    [Authorize, HttpPost]
    public async Task<ActionResult<NoteDto>> UpdateNote([FromBody] NoteDto dto)
    {
        try
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var response = await new UpdateModel(scope).UpdateNote(dto);

            var tokenizer = scope.ServiceProvider.GetRequiredService<ITokenizerService>();
            tokenizer.Update(dto.CommonNoteId, new NoteEntity { Title = dto.TitleRequest, Text = dto.TextRequest });

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, UpdateNoteError);
            return new NoteDto { CommonErrorMessageResponse = UpdateNoteError };
        }
    }
}
