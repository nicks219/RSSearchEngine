using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SearchEngine.Api.Mapping;
using SearchEngine.Service.ApiModels;
using SearchEngine.Service.Configuration;
using SearchEngine.Services;
using static SearchEngine.Api.Messages.ControllerMessages;

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
    public ActionResult SwitchNodeElectionMode()
    {
        _randomElection = !_randomElection;
        return Ok(new { RandomElection = _randomElection });
    }

    /// <summary>
    /// Прочитать заголовок заметки по её идентификатору.
    /// </summary>
    /// <param name="id">Идентификатор заметки.</param>
    [HttpGet(RouteConstants.ReadTitleGetUrl)]
    public async Task<ActionResult> ReadTitleByNoteId(string id)
    {
        try
        {
            var res = await readService.ReadTitleByNoteId(int.Parse(id));
            return Ok(new { res });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ReadTitleByNoteIdError);
            return BadRequest(ReadTitleByNoteIdError);
        }
    }

    /// <summary>
    /// Выбрать следующую либо прочитать конкретную заметку, по отмеченным тегам или идентификатору.
    /// </summary>
    /// <param name="request">Контейнер с отмеченными тегами.</param>
    /// <param name="id">Строка с идентификатором, если требуется.</param>
    /// <returns>Ответ с заметкой.</returns>
    [HttpPost(RouteConstants.ReadNotePostUrl)]
    public async Task<ActionResult<NoteResponse>> GetNextOrSpecificNote([FromBody] NoteRequest? request, [FromQuery] string? id)
    {
        try
        {
            var noteRequestDto = request?.MapToDto();
            if (noteRequestDto?.CheckedTags?.Count == 0)
            {
                // для пустого запроса считаем все теги отмеченными
                // todo: перенести в маппер
                noteRequestDto = noteRequestDto with { CheckedTags = Enumerable.Range(1, 44).ToList() };
            }

            var response = await readService.GetNextOrSpecificNote(noteRequestDto, id, _randomElection);
            return response.MapFromDto();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            logger.LogError(ex, ElectNoteError);
            return new NoteResponse { ErrorMessage = ElectNoteError };
        }
    }

    /// <summary>
    /// Получить полный список тегов.
    /// </summary>
    [HttpGet(RouteConstants.ReadTagsGetUrl)]
    public async Task<ActionResult<NoteResponse>> ReadTagList()
    {
        try
        {
            var response = await readService.ReadEnrichedTagList();
            return response.MapFromDto();
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
    [Authorize, HttpGet(RouteConstants.ReadTagsForCreateAuthGetUrl)]
    [Obsolete("используйте ReadController.ReadTagList")]
    // todo: неудачный рефакторинг, исправить
    public async Task<ActionResult<NoteResponse>> GetStructuredTagListForCreate()
    {
        try
        {
            var response = await ReadTagList();
            return response;
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
    [Authorize, HttpGet(RouteConstants.ReadNoteWithTagsForUpdateAuthGetUrl)]
    // todo: неудачный рефакторинг, исправить
    public async Task<ActionResult<NoteResponse>> GetNoteWithTagsForUpdate(int id)
    {
        try
        {
            var response = await updateService.GetNoteWithTagsForUpdate(id);
            return response.MapFromDto();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, GetNoteWithTagsForUpdateError);
            return new NoteResponse { ErrorMessage = GetNoteWithTagsForUpdateError };
        }
    }
}
