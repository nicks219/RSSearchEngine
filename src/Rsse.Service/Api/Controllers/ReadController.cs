using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SearchEngine.Api.Mapping;
using SearchEngine.Service.ApiModels;
using SearchEngine.Service.Configuration;
using SearchEngine.Services;
using static SearchEngine.Api.Messages.ControllerErrorMessages;

namespace SearchEngine.Api.Controllers;

/// <summary>
/// Контроллер для чтения заметок.
/// </summary>
[ApiController]
[ApiExplorerSettings(IgnoreApi = !Constants.IsDebug)]
public class ReadController(
    ReadService readService,
    UpdateService updateService,
    ILogger<ReadController> logger) : ControllerBase
{
    private static bool _randomElection = true;

    /// <summary>
    /// Переключить режим выбора следующей заметки.
    /// </summary>
    [HttpGet(RouteConstants.ReadElectionGetUrl)]
    public ActionResult<RandomElectionResponse> SwitchNodeElectionMode()
    {
        _randomElection = !_randomElection;
        var response = new RandomElectionResponse(RandomElection: _randomElection);
        return Ok(response);
    }

    /// <summary>
    /// Прочитать заголовок заметки по её идентификатору.
    /// </summary>
    /// <param name="id">Идентификатор заметки.</param>
    /// <param name="ct">Токен отмены.</param>
    [HttpGet(RouteConstants.ReadTitleGetUrl)]
    public async Task<ActionResult<StringResponse>> ReadTitleByNoteId(string id, CancellationToken ct)
    {
        try
        {
            var title = await readService.ReadTitleByNoteId(int.Parse(id), ct);
            var response = new StringResponse(Res: title);
            return Ok(response);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ReadTitleByNoteIdError);
            var response = new StringResponse(Error: ReadTitleByNoteIdError);
            return BadRequest(response);
        }
    }

    /// <summary>
    /// Выбрать следующую либо прочитать конкретную заметку, по отмеченным тегам или идентификатору.
    /// </summary>
    /// <param name="request">Контейнер с отмеченными тегами.</param>
    /// <param name="id">Строка с идентификатором, если требуется.</param>
    /// <param name="ct">Токен отмены.</param>
    /// <returns>Ответ с заметкой.</returns>
    [HttpPost(RouteConstants.ReadNotePostUrl)]
    public async Task<ActionResult<NoteResponse>> GetNextOrSpecificNote(
        [FromBody] NoteRequest? request,
        [FromQuery] string? id,
        CancellationToken ct)
    {
        try
        {
            var noteRequestDto = request?.MapToDto();
            var noteResultDto = await readService.GetNextOrSpecificNote(noteRequestDto, id, _randomElection, ct);
            return noteResultDto.MapFromDto();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ElectNoteError);
            return new NoteResponse { ErrorMessage = ElectNoteError };
        }
    }

    /// <summary>
    /// Получить полный список тегов.
    /// </summary>
    /// <param name="ct">Токен отмены.</param>
    [HttpGet(RouteConstants.ReadTagsGetUrl)]
    public async Task<ActionResult<NoteResponse>> ReadTagList(CancellationToken ct)
    {
        try
        {
            var noteResultDto = await readService.ReadEnrichedTagList(ct);
            return noteResultDto.MapFromDto();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ReadTagListError);
            return new NoteResponse { ErrorMessage = ReadTagListError };
        }
    }

    /// <summary>
    /// Получить список тегов, под авторизацией.
    /// </summary>
    /// <param name="ct">Токен отмены.</param>
    [Authorize, HttpGet(RouteConstants.ReadTagsForCreateAuthGetUrl)]
    [Obsolete("используйте ReadController.ReadTagList")]
    // todo: неудачный рефакторинг, исправить
    public async Task<ActionResult<NoteResponse>> GetStructuredTagListForCreate(CancellationToken ct)
    {
        try
        {
            var noteResponse = await ReadTagList(ct);
            return noteResponse;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, GetTagListForCreateError);
            return new NoteResponse { ErrorMessage = GetTagListForCreateError };
        }
    }

    /// <summary>
    /// Получить обновляемую заметку.
    /// </summary>
    /// <param name="id">Идентификатор обновляемой заметки.</param>
    /// <param name="ct">Токен отмены.</param>
    [Authorize, HttpGet(RouteConstants.ReadNoteWithTagsForUpdateAuthGetUrl)]
    // todo: неудачный рефакторинг, исправить
    public async Task<ActionResult<NoteResponse>> GetNoteWithTagsForUpdate(int id, CancellationToken ct)
    {
        try
        {
            var noteResultDto = await updateService.GetNoteWithTagsForUpdate(id, ct);
            return noteResultDto.MapFromDto();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, GetNoteWithTagsForUpdateError);
            return new NoteResponse { ErrorMessage = GetNoteWithTagsForUpdateError };
        }
    }
}
