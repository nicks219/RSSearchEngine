using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SearchEngine.Data.Dto;
using SearchEngine.Service.Models;

namespace SearchEngine.Controllers;

[ApiController]
[Route("api/read")]

public class ReadController : ControllerBase
{
    public const string ElectNoteError = $"[{nameof(ReadController)}: {nameof(ElectNote)} error]";
    private const string ReadTitleByNoteIdError = $"[{nameof(ReadController)}: {nameof(ReadTitleByNoteId)} error]";
    private const string ReadTagListError = $"[{nameof(ReadController)}: {nameof(ReadTagList)} error]";

    private static bool _randomElection = true;
    private readonly ILogger<ReadController> _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public ReadController(IServiceScopeFactory serviceScopeFactory, ILogger<ReadController> logger)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
    }

    [HttpGet("election")]
    public ActionResult ChangeTextElectionMethod()
    {
        _randomElection = !_randomElection;
        return Ok(new { RandomElection = _randomElection });
    }

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
            return new NoteDto { ErrorMessageResponse = ReadTagListError };
        }
    }

    [HttpPost]
    public async Task<ActionResult<NoteDto>> ElectNote([FromBody] NoteDto? dto, [FromQuery] string? id)
    {
        try
        {
            if (dto?.SongGenres?.Count == 0)
            {
                dto.SongGenres = Enumerable.Range(1, 44).ToList();
            }

            using var scope = _serviceScopeFactory.CreateScope();
            var model = await new ReadModel(scope).ElectNote(dto, id, _randomElection);
            return model;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ElectNoteError);
            return new NoteDto { ErrorMessageResponse = ElectNoteError };
        }
    }

    // CORS ручная настройка
    // [HttpOptions]
    // public ActionResult Options()
    // {
    //    HttpContext.Response.Headers.Add("Access-Control-Allow-Origin", "http://localhost:3000");
    //    HttpContext.Response.Headers.Add("Access-Control-Allow-Credentials", "true");
    //    HttpContext.Response.Headers.Add("Access-Control-Allow-Methods", "OPTIONS");
    //    HttpContext.Response.Headers.Add("Access-Control-Allow-Headers", "Content-type");
    //    return Ok();
    // }
}
