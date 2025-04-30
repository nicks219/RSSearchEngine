using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SearchEngine.Common;
using SearchEngine.Data.Dto;
using SearchEngine.Data.Entities;
using SearchEngine.Engine.Contracts;
using SearchEngine.Managers;
using static SearchEngine.Common.ControllerMessages;

namespace SearchEngine.Controllers;

/// <summary>
/// Контроллер для обновления заметки
/// </summary>
[Authorize, Route("api/update"), ApiController]
[ApiExplorerSettings(IgnoreApi = !Constants.IsDebug)]
public class UpdateController(ILogger<UpdateController> logger)
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
            var scopedProvider = HttpContext.RequestServices;
            return await new UpdateManager(scopedProvider).GetOriginalNote(id);
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
            var scopedProvider = HttpContext.RequestServices;
            var response = await new UpdateManager(scopedProvider).UpdateNote(dto);

            var tokenizer = scopedProvider.GetRequiredService<ITokenizerService>();
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
