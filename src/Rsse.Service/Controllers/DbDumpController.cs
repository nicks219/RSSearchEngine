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

public class DbDumpController : ControllerBase
{
    private const string CreateError = $"[{nameof(DbDumpController)}] {nameof(CreateDump)} error";
    private const string RestoreError = $"[{nameof(DbDumpController)}] {nameof(RestoreFromDump)} error";

    private readonly ILogger<DbDumpController> _logger;
    private readonly IDbMigrator _migrator;
    private readonly ITokenizerService _tokenizer;

    public DbDumpController(ILogger<DbDumpController> logger, IDbMigrator migrator, ITokenizerService tokenizer)
    {
        _logger = logger;
        _migrator = migrator;
        _tokenizer = tokenizer;
    }

    [HttpGet("/create")]
    public IActionResult CreateDump(string? fileName)
    {
        try
        {
            var result = _migrator.Create(fileName);

            return new OkObjectResult(new { Res = Path.GetFileName(result) });
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, CreateError);
            return BadRequest(CreateError);
        }
    }

    [HttpGet("/restore")]
    public IActionResult RestoreFromDump(string? fileName)
    {
        try
        {
            var result = _migrator.Restore(fileName);
            _tokenizer.Initialize();

            return new OkObjectResult(new { Res = Path.GetFileName(result) });
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, RestoreError);
            return BadRequest(RestoreError);
        }
    }
}
