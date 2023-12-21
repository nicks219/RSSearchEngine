using System;
using System.IO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SearchEngine.Infrastructure;
using SearchEngine.Infrastructure.Tokenizer.Contracts;

namespace SearchEngine.Controllers;

[Authorize]
[Route("backup")]
[ApiController]

public class BackupController : ControllerBase
{
    private readonly ILogger<BackupController> _logger;
    private readonly IDbMigrator _migrator;
    private readonly ITokenizerService _tokenizer;

    public BackupController(ILogger<BackupController> logger, IDbMigrator migrator, ITokenizerService tokenizer)
    {
        _logger = logger;
        _migrator = migrator;
        _tokenizer = tokenizer;
    }

    [HttpGet("/create")]
    public IActionResult CreateBackup(string? fileName)
    {
        try
        {
            var result = _migrator.Create(fileName);

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
            var result = _migrator.Restore(fileName);
            _tokenizer.Initialize();

            return new OkObjectResult(new { Res = Path.GetFileName(result) });
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, $"[{nameof(BackupController)}] {nameof(RestoreFromBackup)} error");
            return BadRequest($"[{nameof(BackupController)}] {nameof(RestoreFromBackup)} error: {exception}");
        }
    }
}
