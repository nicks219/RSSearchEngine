using System;
using System.IO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SearchEngine.Engine.Contracts;
using SearchEngine.Tools.Migrator;

namespace SearchEngine.Controllers;

/// <summary>
/// Контроллер для работы с миграциями бд
/// </summary>

[Authorize, Route("backup"), ApiController]

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

    /// <summary>
    /// Создать дамп бд
    /// </summary>
    /// <param name="fileName">имя файла с дампом, либо выбор имени из ротации</param>
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

    /// <summary>
    /// Накатить дамп
    /// </summary>
    /// <param name="fileName">имя файла с дампом, либо выбор имени из ротации</param>
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
