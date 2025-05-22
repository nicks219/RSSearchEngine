using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SearchEngine.Api.Mapping;
using SearchEngine.Data.Dto;
using SearchEngine.Service.ApiModels;
using SearchEngine.Service.Configuration;
using SearchEngine.Service.Contracts;
using SearchEngine.Services;
using static SearchEngine.Api.Messages.ControllerMessages;

namespace SearchEngine.Api.Controllers;

/// <summary>
/// Контроллер для обновления заметки.
/// </summary>
[Authorize, ApiController]
[ApiExplorerSettings(IgnoreApi = !Constants.IsDebug)]
public class UpdateController(
    ITokenizerService tokenizer,
    UpdateService updateService,
    ILogger<UpdateController> logger) : ControllerBase
{
    /// <summary>
    /// Обновить заметку.
    /// </summary>
    /// <param name="request">Контейнер с данными для обновления.</param>
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
