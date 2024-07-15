using System;
using System.IO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SearchEngine.Common.Auth;
using SearchEngine.Engine.Contracts;
using SearchEngine.Tools.MigrationAssistant;

namespace SearchEngine.Controllers;

/// <summary>
/// Контроллер для работы с миграциями бд
/// </summary>

[Authorize, Route("migration"), ApiController]

public class MigrationController : ControllerBase
{
    private const string CreateError = $"[{nameof(MigrationController)}] {nameof(CreateDump)} error";
    private const string RestoreError = $"[{nameof(MigrationController)}] {nameof(RestoreFromDump)} error";

    private readonly ILogger<MigrationController> _logger;
    private readonly IDbMigrator _migrator;
    private readonly ITokenizerService _tokenizer;

    public MigrationController(ILogger<MigrationController> logger, IDbMigrator migrator, ITokenizerService tokenizer)
    {
        _logger = logger;
        _migrator = migrator;
        _tokenizer = tokenizer;
    }

    /// <summary>
    /// Создать дамп бд.
    /// </summary>
    /// <param name="fileName">имя файла с дампом, либо выбор имени из ротации</param>
    [HttpGet("create")]
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
    /// Накатить дамп.
    /// </summary>
    /// <param name="fileName">имя файла с дампом, либо выбор имени из ротации</param>
    [HttpGet("restore")]
    [Authorize(Constants.FullAccessPolicyName)]
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
