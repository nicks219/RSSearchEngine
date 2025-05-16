using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SearchEngine.Api.Mapping;
using SearchEngine.Domain.ApiModels;
using SearchEngine.Domain.Configuration;
using SearchEngine.Domain.Contracts;
using SearchEngine.Domain.Managers;
using static SearchEngine.Domain.Configuration.ControllerMessages;

namespace SearchEngine.Api.Controllers;

/// <summary>
/// Контроллер для получения заметок
/// </summary>
[ApiController]
[ApiExplorerSettings(IgnoreApi = !Constants.IsDebug)]
public class ReadController(
    IDataRepository repo,
    ILoggerFactory loggerFactory) : ControllerBase
{
    private readonly ILogger<ReadController> _logger = loggerFactory.CreateLogger<ReadController>();
    private readonly ILogger<ReadManager> _readManagerLogger = loggerFactory.CreateLogger<ReadManager>();

    private static bool _randomElection = true;

    /// <summary>
    /// Переключить режим выбора следующей заметки
    /// </summary>
    [HttpGet(RouteConstants.ReadElectionGetUrl)]
    public ActionResult SwitchNodeElectionMode()
    {
        _randomElection = !_randomElection;
        return Ok(new { RandomElection = _randomElection });
    }

    /// <summary>
    /// Прочитать заголовок заметки по её идентификатору
    /// </summary>
    /// <param name="id">идентификатор заметки</param>
    [HttpGet(RouteConstants.ReadTitleGetUrl)]
    public async Task<ActionResult> ReadTitleByNoteId(string id)
    {
        try
        {
            var res = await new ReadManager(repo, _readManagerLogger).ReadTitleByNoteId(int.Parse(id));
            return Ok(new { res });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ReadTitleByNoteIdError);
            return BadRequest(ReadTitleByNoteIdError);
        }
    }

    /// <summary>
    /// Выбрать следующую либо прочитать конкретную заметку, по отмеченным тегам или идентификатору
    /// </summary>
    /// <param name="request">данные с отмеченными тегами</param>
    /// <param name="id">строка с идентификатором, если требуется</param>
    /// <returns>ответ с заметкой</returns>
    [HttpPost(RouteConstants.ReadNotePostUrl)]
    public async Task<ActionResult<NoteResponse>> GetNextOrSpecificNote([FromBody] NoteRequest? request, [FromQuery] string? id)
    {
        try
        {
            var dto = request?.MapToDto();
            if (dto?.TagsCheckedRequest?.Count == 0)
            {
                // для пустого запроса считаем все теги отмеченными
                dto.TagsCheckedRequest = Enumerable.Range(1, 44).ToList();
            }

            var response = await new ReadManager(repo, _readManagerLogger).GetNextOrSpecificNote(dto, id, _randomElection);
            return response.MapFromDto();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            _logger.LogError(ex, ElectNoteError);
            return new NoteResponse { CommonErrorMessageResponse = ElectNoteError };
        }
    }

    /// <summary>
    /// Получить список тегов
    /// </summary>
    [HttpGet(RouteConstants.ReadTagsGetUrl)]
    public async Task<ActionResult<NoteResponse>> ReadTagList()
    {
        try
        {
            var response = await new ReadManager(repo, _readManagerLogger).ReadTagList();
            return response.MapFromDto();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ReadTagListError);
            return new NoteResponse { CommonErrorMessageResponse = ReadTagListError };
        }
    }

    /// <summary>
    /// Получить список тегов
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
            var logger = loggerFactory.CreateLogger<CreateManager>();
            logger.LogError(ex, GetTagListForCreateError);
            return new NoteResponse { CommonErrorMessageResponse = GetTagListForCreateError };
        }
    }

    /// <summary>
    /// Получить обновляемую заметку
    /// </summary>
    /// <param name="id">идентификатор обновляемой заметки</param>
    [Authorize, HttpGet(RouteConstants.ReadNoteWithTagsForUpdateAuthGetUrl)]
    // todo: неудачный рефакторинг, исправить
    public async Task<ActionResult<NoteResponse>> GetNoteWithTagsForUpdate(int id)
    {
        var logger = loggerFactory.CreateLogger<UpdateManager>();
        try
        {
            var response = await new UpdateManager(repo, logger).GetNoteWithTagsForUpdate(id);
            return response.MapFromDto();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, GetNoteWithTagsForUpdateError);
            return new NoteResponse { CommonErrorMessageResponse = GetNoteWithTagsForUpdateError };
        }
    }
}
