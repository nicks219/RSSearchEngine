using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SearchEngine.Data.Dto;
using SearchEngine.Data.Entities;
using SearchEngine.Engine.Contracts;
using SearchEngine.Models;
using static SearchEngine.Common.ControllerMessages;

namespace SearchEngine.Controllers;

/// <summary>
/// Контроллер для обновления заметки
/// </summary>
[Route("api/update"), ApiController]
public class UpdateController(IServiceScopeFactory serviceScopeFactory, ILogger<UpdateController> logger)
    : ControllerBase
{
    /// <summary>
    /// Получить обновляемую заметку
    /// </summary>
    /// <param name="id">идентификатор обновляемой заметки</param>
    [HttpGet]
    public async Task<ActionResult<NoteDto>> GetInitialNote(int id)
    {
        try
        {
            using var scope = serviceScopeFactory.CreateScope();
            return await new UpdateModel(scope).GetOriginalNote(id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, GetInitialNoteError);
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
            using var scope = serviceScopeFactory.CreateScope();
            var response = await new UpdateModel(scope).UpdateNote(dto);

            var tokenizer = scope.ServiceProvider.GetRequiredService<ITokenizerService>();
            tokenizer.Update(dto.CommonNoteId, new NoteEntity { Title = dto.TitleRequest, Text = dto.TextRequest });

            return response;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, UpdateNoteError);
            return new NoteDto { CommonErrorMessageResponse = UpdateNoteError };
        }
    }
}
