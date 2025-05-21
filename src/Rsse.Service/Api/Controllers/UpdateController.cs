using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SearchEngine.Api.Mapping;
using SearchEngine.Domain.ApiModels;
using SearchEngine.Domain.Configuration;
using SearchEngine.Domain.Contracts;
using SearchEngine.Domain.Dto;
using SearchEngine.Domain.Entities;
using SearchEngine.Domain.Services;
using static SearchEngine.Domain.Configuration.ControllerMessages;

namespace SearchEngine.Api.Controllers;

/// <summary>
/// Контроллер для обновления заметки
/// </summary>
[Authorize, ApiController]
[ApiExplorerSettings(IgnoreApi = !Constants.IsDebug)]
public class UpdateController(
    ITokenizerService tokenizer,
    UpdateService updateService,
    ILogger<UpdateController> logger) : ControllerBase
{
    /// <summary>
    /// Обновить заметку
    /// </summary>
    /// <param name="request">данные для обновления</param>
    [Authorize, HttpPut(RouteConstants.UpdateNotePutUrl)]
    public async Task<ActionResult<NoteResponse>> UpdateNote([FromBody] NoteRequest request)
    {
        try
        {
            var dto = request.MapToDto();
            var response = await updateService.UpdateNote(dto);
            var entity = new TextRequestDto
            {
                Title = dto.Title,
                Text = dto.Text
            };

            await tokenizer.Update(dto.NoteIdExchange, entity);

            return response.MapFromDto();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, UpdateNoteError);
            return new NoteResponse { ErrorMessage = UpdateNoteError };
        }
    }
}
