using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using SearchEngine.Data.Dto;
using SearchEngine.Service.ApiModels;
using SearchEngine.Service.Configuration;
using SearchEngine.Service.Contracts;
using SearchEngine.Service.Mapping;
using SearchEngine.Services;

namespace SearchEngine.Api.Controllers;

/// <summary>
/// Контроллер для обновления заметки.
/// </summary>
[Authorize, ApiController]
[ApiExplorerSettings(IgnoreApi = !Constants.IsDebug)]
public class UpdateController(
    IHostApplicationLifetime lifetime,
    ITokenizerService tokenizerService,
    UpdateService updateService) : ControllerBase
{
    /// <summary>
    /// Обновить заметку.
    /// </summary>
    /// <param name="request">Контейнер с данными для обновления.</param>
    [Authorize, HttpPut(RouteConstants.UpdateNotePutUrl)]
    public async Task<ActionResult<NoteResponse>> UpdateNote(
        [FromBody][Required(AllowEmptyStrings = false)] NoteRequest request)
    {
        var stoppingToken = lifetime.ApplicationStopping;
        if (stoppingToken.IsCancellationRequested) return StatusCode(503);
        if (string.IsNullOrWhiteSpace(request.Title) || request.Text == null) return StatusCode(400);

        var noteRequest = request.MapToDto();
        var noteResultDto = await updateService.UpdateNote(noteRequest, stoppingToken);
        var textRequestDto = new TextRequestDto
        {
            Title = noteRequest.Title,
            Text = noteRequest.Text
        };

        await tokenizerService.Update(noteRequest.NoteIdExchange, textRequestDto, stoppingToken);

        var noteResponse = noteResultDto.MapFromDto();

        return Ok(noteResponse);
    }
}
