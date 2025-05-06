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
    ILogger<ReadController> logger,
    ILogger<ReadManager> managerLogger) : ControllerBase
{
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
    public ActionResult ReadTitleByNoteId(string id)
    {
        try
        {
            var res = new ReadManager(repo, managerLogger).ReadTitleByNoteId(int.Parse(id));
            return Ok(new { res });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ReadTitleByNoteIdError);
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

            var response = await new ReadManager(repo, managerLogger).GetNextOrSpecificNote(dto, id, _randomElection);
            return response.MapFromDto();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            logger.LogError(ex, ElectNoteError);
            return new NoteResponse { CommonErrorMessageResponse = ElectNoteError };
        }
    }

    /// <summary>
    /// Получить список тегов
    /// </summary>
    [HttpGet(RouteConstants.ReadGetTagsUrl)]
    public async Task<ActionResult<NoteResponse>> ReadTagList()
    {
        try
        {
            var response = await new ReadManager(repo, managerLogger).ReadTagList();
            return response.MapFromDto();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ReadTagListError);
            return new NoteResponse { CommonErrorMessageResponse = ReadTagListError };
        }
    }

    /// <summary>
    /// Получить обновляемую заметку
    /// </summary>
    /// <param name="id">идентификатор обновляемой заметки</param>
    [Authorize, HttpGet(RouteConstants.UpdateGetNoteWithTagsUrl)]
    public async Task<ActionResult<NoteResponse>> GetInitialNoteForUpdate(int id)
    {
        try
        {
            var response = await new UpdateManager(repo, managerLogger).GetOriginalNote(id);
            return response.MapFromDto();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, GetInitialNoteError);
            return new NoteResponse { CommonErrorMessageResponse = GetInitialNoteError };
        }
    }

    /// <summary>
    /// Получить список тегов
    /// </summary>
    [Authorize, HttpGet(RouteConstants.CreateGetTagsAuthorizedUrl)]
    [Obsolete("используйте ReadController.ReadTagList")]
    public async Task<ActionResult<NoteResponse>> GetStructuredTagListForCreate()
    {
        try
        {
            var model = new CreateManager(repo, managerLogger);
            var response = await model.ReadStructuredTagList();
            return response.MapFromDto();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, GetTagListError);
            return new NoteResponse { CommonErrorMessageResponse = GetTagListError };
        }
    }
}
