using Microsoft.AspNetCore.Mvc;
using RandomSongSearchEngine.Infrastructure;

namespace RandomSongSearchEngine.Controllers;

[ApiController]
[Route("backup")]
public class BackupController : ControllerBase
{
    private readonly ILogger<BackupController> _logger;
    private readonly IMysqlBackup _backup;

    public BackupController(ILogger<BackupController> logger, IMysqlBackup backup)
    {
        _logger = logger;
        _backup = backup;
    }

    [HttpGet("/create")]
    public IActionResult CreateBackup(string? fileName)
    {
        try
        {
            var result = _backup.Backup(fileName);
            return Ok(new {result});
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "[Backup Controller] create error");
            return BadRequest("[Backup Controller] create error");
        }
    }
    
    [HttpGet("/restore")]
    public IActionResult RestoreFromBackup(string? fileName)
    {
        try
        {
            var result = _backup.Restore(fileName);
            return Ok(new {result});
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "[Backup Controller] restore error");
            return BadRequest("[Backup Controller] restore error");
        }
    }
}