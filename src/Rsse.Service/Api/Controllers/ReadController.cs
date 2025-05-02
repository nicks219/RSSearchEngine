using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SearchEngine.Domain.Configuration;
using SearchEngine.Domain.Dto;
using SearchEngine.Domain.Managers;
using static SearchEngine.Domain.Configuration.ControllerMessages;

namespace SearchEngine.Api.Controllers;

/// <summary>
/// Контроллер для получения заметок
/// </summary>
[ApiController, Route("api/read")]
[ApiExplorerSettings(IgnoreApi = !Constants.IsDebug)]
public class ReadController(ILogger<ReadController> logger) : ControllerBase
{
    private static bool _randomElection = true;

    /// <summary>
    /// Переключить режим выбора следующей заметки
    /// </summary>
    [HttpGet("election")]
    public ActionResult SwitchNodeElectionMode()
    {
        _randomElection = !_randomElection;
        return Ok(new { RandomElection = _randomElection });
    }

    /// <summary>
    /// Прочитать заголовок заметки по её идентификатору
    /// </summary>
    /// <param name="id">идентификатор заметки</param>
    [HttpGet("title")]
    public ActionResult ReadTitleByNoteId(string id)
    {
        try
        {
            var scopedProvider = HttpContext.RequestServices;
            var res = new ReadManager(scopedProvider).ReadTitleByNoteId(int.Parse(id));
            return Ok(new { res });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ReadTitleByNoteIdError);
            return BadRequest(ReadTitleByNoteIdError);
        }
    }

    /// <summary>
    /// Получить список тегов
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<NoteDto>> ReadTagList()
    {
        try
        {
            var scopedProvider = HttpContext.RequestServices;
            return await new ReadManager(scopedProvider).ReadTagList();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ReadTagListError);
            return new NoteDto { CommonErrorMessageResponse = ReadTagListError };
        }
    }

    /// <summary>
    /// Выбрать следующую либо прочитать конкретную заметку, по отмеченным тегам или идентификатору
    /// </summary>
    /// <param name="dto">данные с отмеченными тегами</param>
    /// <param name="id">строка с идентификатором, если требуется</param>
    /// <returns>ответ с заметкой</returns>
    [HttpPost]
    public async Task<ActionResult<NoteDto>> GetNextOrSpecificNote([FromBody] NoteDto? dto, [FromQuery] string? id)
    {
        try
        {
            if (dto?.TagsCheckedRequest?.Count == 0)
            {
                // для пустого запроса считаем все теги отмеченными
                dto.TagsCheckedRequest = Enumerable.Range(1, 44).ToList();
            }

            var scopedProvider = HttpContext.RequestServices;
            var model = await new ReadManager(scopedProvider).GetNextOrSpecificNote(dto, id, _randomElection);
            return model;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            logger.LogError(ex, ElectNoteError);
            return new NoteDto { CommonErrorMessageResponse = ElectNoteError };
        }
    }
}
