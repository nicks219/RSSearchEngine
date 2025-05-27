using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SearchEngine.Api.Mapping;
using SearchEngine.Service.ApiModels;
using SearchEngine.Service.Configuration;
using SearchEngine.Services;

namespace SearchEngine.Api.Controllers;

/// <summary>
/// Контроллер для чтения заметок.
/// </summary>
[ApiController]
[ApiExplorerSettings(IgnoreApi = !Constants.IsDebug)]
public class ReadController(
    ReadService readService,
    UpdateService updateService) : ControllerBase
{
    private static bool _randomElection = true;

    /// <summary>
    /// Переключить режим выбора следующей заметки.
    /// </summary>
    [HttpGet(RouteConstants.ReadElectionGetUrl)]
    public ActionResult<RandomElectionResponse> SwitchNodeElectionMode(CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested) return StatusCode(503);

        _randomElection = !_randomElection;
        var response = new RandomElectionResponse(RandomElection: _randomElection);
        return Ok(response);
    }

    /// <summary>
    /// Прочитать заголовок заметки по её идентификатору.
    /// </summary>
    /// <param name="id">Идентификатор заметки.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    [HttpGet(RouteConstants.ReadTitleGetUrl)]
    public async Task<ActionResult<StringResponse>> ReadTitleByNoteId(string id, CancellationToken cancellationToken)
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

        var noteRequestDto = request?.MapToDto();
        var noteResultDto = await readService.GetNextOrSpecificNote(noteRequestDto, id, _randomElection, cancellationToken);
        var noteResponse = noteResultDto.MapFromDto();
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
    /// Получить обновляемую заметку.
    /// </summary>
    /// <param name="id">Идентификатор обновляемой заметки.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    [Authorize, HttpGet(RouteConstants.ReadNoteWithTagsForUpdateAuthGetUrl)]
    // todo: неудачный рефакторинг, исправить
    public async Task<ActionResult<NoteResponse>> GetNoteWithTagsForUpdate(int id, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested) return StatusCode(503);

        var noteResultDto = await updateService.GetNoteWithTagsForUpdate(id, cancellationToken);
        var noteResponse = noteResultDto.MapFromDto();
        return Ok(noteResponse);
    }
}
