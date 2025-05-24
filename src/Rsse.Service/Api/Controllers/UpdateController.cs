using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SearchEngine.Api.Mapping;
using SearchEngine.Data.Dto;
using SearchEngine.Service.ApiModels;
using SearchEngine.Service.Configuration;
using SearchEngine.Service.Contracts;
using SearchEngine.Services;
using static SearchEngine.Api.Messages.ControllerErrorMessages;

namespace SearchEngine.Api.Controllers;

/// <summary>
/// Контроллер для обновления заметки.
/// </summary>
[Authorize, ApiController]
[ApiExplorerSettings(IgnoreApi = !Constants.IsDebug)]
public class UpdateController(
    IHostApplicationLifetime lifetime,
    ITokenizerService tokenizerService,
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
        var ct = lifetime.ApplicationStopping;
        try
        {
            var noteRequest = request.MapToDto();
            var noteResultDto = await updateService.UpdateNote(noteRequest, ct);
            var textRequestDto = new TextRequestDto
            {
                Title = noteRequest.Title,
                Text = noteRequest.Text
            };

            await tokenizerService.Update(noteRequest.NoteIdExchange, textRequestDto, ct);

            return noteResultDto.MapFromDto();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, UpdateNoteError);
            return new NoteResponse { ErrorMessage = UpdateNoteError };
        }
    }
}
