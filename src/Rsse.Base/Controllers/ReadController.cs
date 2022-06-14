using Microsoft.AspNetCore.Mvc;
using RandomSongSearchEngine.Data.DTO;
using RandomSongSearchEngine.Service.Models;

namespace RandomSongSearchEngine.Controllers;

[ApiController]
[Route("api/read")]

public class ReadController : ControllerBase
{
    private readonly ILogger<ReadController> _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public ReadController(IServiceScopeFactory serviceScopeFactory, ILogger<ReadController> logger)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
    }

    [HttpGet("title")]
    public ActionResult GetTitleById(string id)
    {
        try
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var res = new ReadModel(scope).ReadSongTitleById(int.Parse(id));
            return Ok(new {res});
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[ReadController: OnGetTitle Error]");
            return BadRequest("[ReadController: OnGetTitle Error]");
        }
    }

    [HttpGet]
    public async Task<ActionResult<SongDto>> OnGetGenreListAsync()
    {
        try
        {
            using var scope = _serviceScopeFactory.CreateScope();
            return await new ReadModel(scope).ReadGenreListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[ReadController: OnGet Error]");
            return new SongDto() {ErrorMessageResponse = "[ReadController: OnGet Error]"};
        }
    }

    [HttpPost]
    public async Task<ActionResult<SongDto>> GetRandomSongAsync([FromBody] SongDto? dto)
    {
        if (dto?.SongGenres?.Count == 0)
        {
            dto.SongGenres = Enumerable.Range(1, 44).ToList();
        }

        try
        {
            using var scope = _serviceScopeFactory.CreateScope();
            return await new ReadModel(scope).ReadRandomSongAsync(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[ReadController: OnPost Error]");
            return new SongDto() {ErrorMessageResponse = "[ReadController: OnPost Error]"};
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