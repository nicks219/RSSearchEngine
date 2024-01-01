using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SearchEngine.Data.Dto;
using SearchEngine.Models;

namespace SearchEngine.Controllers;

/// <summary>
/// Контроллер для получения заметок
/// </summary>

[ApiController, Route("api/read")]

public class ReadController : ControllerBase
{
    public const string ElectNoteError = $"[{nameof(ReadController)}] {nameof(GetNextOrSpecificNote)} error";
    private const string ReadTitleByNoteIdError = $"[{nameof(ReadController)}] {nameof(ReadTitleByNoteId)} error";
    private const string ReadTagListError = $"[{nameof(ReadController)}] {nameof(ReadTagList)} error";

    private static bool _randomElection = true;
    private readonly ILogger<ReadController> _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public ReadController(IServiceScopeFactory serviceScopeFactory, ILogger<ReadController> logger)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
    }

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
            using var scope = _serviceScopeFactory.CreateScope();
            var res = new ReadModel(scope).ReadTitleByNoteId(int.Parse(id));
            return Ok(new { res });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ReadTitleByNoteIdError);
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
            using var scope = _serviceScopeFactory.CreateScope();
            return await new ReadModel(scope).ReadTagList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ReadTagListError);
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
                dto.TagsCheckedRequest = Enumerable.Range(1, 44).ToList();
            }

            using var scope = _serviceScopeFactory.CreateScope();
            var model = await new ReadModel(scope).GetNextOrSpecificNote(dto, id, _randomElection);
            return model;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ElectNoteError);
            return new NoteDto { CommonErrorMessageResponse = ElectNoteError };
        }
    }
}
