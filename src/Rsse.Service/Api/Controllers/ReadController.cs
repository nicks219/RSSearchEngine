using System;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Rsse.Domain.Service.Api;
using Rsse.Domain.Service.ApiModels;
using Rsse.Domain.Service.Configuration;
using Rsse.Domain.Service.Mapping;
using Serilog;

namespace Rsse.Api.Controllers;

/// <summary>
/// Контроллер для чтения заметок.
/// </summary>
[ApiController]
[ApiExplorerSettings(IgnoreApi = !Constants.IsDebug)]
public class ReadController(
    ReadService readService,
    UpdateService updateService,
    IOptionsMonitor<ElectionTypeOptions> electionOptions) : ControllerBase
{
    private readonly ElectionTypeOptions _electionTypeOptions = electionOptions.CurrentValue;

    /// <summary>
    /// Переключить режим выбора следующей заметки.
    /// </summary>
    [HttpGet(RouteConstants.ReadElectionGetUrl)]
    public ActionResult<RandomElectionResponse> SwitchNodeElectionMode(
        [FromQuery][Required(AllowEmptyStrings = false)] ElectionType electionType,
        CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested) return StatusCode(503);

        _electionTypeOptions.ElectionType = electionType;
        var response = new RandomElectionResponse(ElectionType: _electionTypeOptions.ElectionType);
        return Ok(response);
    }

    /// <summary>
    /// Прочитать заголовок заметки по её идентификатору.
    /// </summary>
    /// <param name="id">Идентификатор заметки.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    [HttpGet(RouteConstants.ReadTitleGetUrl)]
    public async Task<ActionResult<StringResponse>> ReadTitleByNoteId(
        [FromQuery][Required(AllowEmptyStrings = false)] string id,
        CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested) return StatusCode(503);

        var title = await readService.ReadTitleByNoteId(int.Parse(id), cancellationToken);
        var response = new StringResponse(Res: title);
        return Ok(response);
    }

    /// <summary>
    /// Выбрать следующую либо прочитать конкретную заметку, по отмеченным тегам или идентификатору.
    /// </summary>
    /// <param name="request">Контейнер с отмеченными тегами.</param>
    /// <param name="id">Строка с идентификатором, если требуется.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Ответ с заметкой.</returns>
    [HttpPost(RouteConstants.ReadNotePostUrl)]
    public async Task<ActionResult<NoteResponse>> GetNextOrSpecificNote(
        [FromBody] NoteRequest? request,
        [FromQuery] string? id,
        CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested) return StatusCode(503);

        var electionType = _electionTypeOptions.ElectionType;
        var noteRequestDto = request?.MapToDto();
        var noteResultDto = await readService.GetNextOrSpecificNote(noteRequestDto, id, electionType, cancellationToken);
        var noteResponse = noteResultDto.MapFromDto();

        Log.Debug("Elected id: {ElectedId}", noteResponse.NoteIdExchange);
        return Ok(noteResponse);
    }

    /// <summary>
    /// Получить полный список тегов.
    /// </summary>
    /// <param name="cancellationToken">Токен отмены.</param>
    [HttpGet(RouteConstants.ReadTagsGetUrl)]
    public async Task<ActionResult<NoteResponse>> ReadTagList(CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested) return StatusCode(503);

        var noteResultDto = await readService.ReadEnrichedTagList(cancellationToken);
        var response = noteResultDto.MapFromDto();
        return Ok(response);
    }

    /// <summary>
    /// Получить список тегов, под авторизацией.
    /// </summary>
    /// <param name="cancellationToken">Токен отмены.</param>
    [Authorize, HttpGet(RouteConstants.ReadTagsForCreateAuthGetUrl)]
    [Obsolete("используйте ReadController.ReadTagList")]
    // todo: неудачный рефакторинг, исправить
    public async Task<ActionResult<NoteResponse>> GetStructuredTagListForCreate(CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested) return StatusCode(503);

        // Отдаётся ответ, полученный из ручки, а не из сервиса.
        var actionNoteResponse = await ReadTagList(cancellationToken);
        return actionNoteResponse;
    }

    /// <summary>
    /// Получить обновляемую заметку, под авторизацией.
    /// </summary>
    /// <param name="id">Идентификатор обновляемой заметки.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    [Authorize, HttpGet(RouteConstants.ReadNoteWithTagsForUpdateAuthGetUrl)]
    // todo: неудачный рефакторинг, исправить
    public async Task<ActionResult<NoteResponse>> GetNoteWithTagsForUpdate(
        [FromQuery][Required] int id, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested) return StatusCode(503);

        var noteResultDto = await updateService.GetNoteWithTagsForUpdate(id, cancellationToken);
        var noteResponse = noteResultDto.MapFromDto();
        return Ok(noteResponse);
    }
}
