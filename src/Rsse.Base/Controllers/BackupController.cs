using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RandomSongSearchEngine.Infrastructure;

namespace RandomSongSearchEngine.Controllers;

[Authorize]
[Route("backup")]
[ApiController]

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
            /*var response = _context.HttpContext?.Response;

            if (response != null)
            {
                response.Clear();
                var contentType = "text/plain";
                //contentType = "application/octet-stream";

                //response.ContentType = contentType;
                
                //response.Headers.Add("Content-Disposition", "attachment; filename=" + result + ";");

                //response.SendFileAsync(result).GetAwaiter().GetResult();
                var path = Path.GetFullPath(result);
                var name = Path.GetFileName(path);
                
                return File(System.IO.File.ReadAllBytes(path), contentType, name);

                //return new PhysicalFileResult(path, contentType);// EmptyResult();
            }*/

            return new OkObjectResult(new { Res = Path.GetFileName(result) });
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, $"[{nameof(BackupController)}] {nameof(CreateBackup)} error");
            return BadRequest($"[{nameof(BackupController)}] {nameof(CreateBackup)} error: {exception}");
        }
    }
    
    [HttpGet("/restore")]
    public IActionResult RestoreFromBackup(string? fileName)
    {
        try
        {
            var result = _backup.Restore(fileName);
            return new OkObjectResult(new { Res = Path.GetFileName(result) });
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, $"[{nameof(BackupController)}] {nameof(RestoreFromBackup)} error");
            return BadRequest($"[{nameof(BackupController)}] {nameof(RestoreFromBackup)} error: {exception}");
        }
    }
}