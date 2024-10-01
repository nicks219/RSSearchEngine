using System;
using System.IO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SearchEngine.Common.Auth;
using SearchEngine.Engine.Contracts;
using SearchEngine.Tools.MigrationAssistant;
using static SearchEngine.Common.ControllerMessages;

namespace SearchEngine.Controllers;

/// <summary>
/// Контроллер для работы с миграциями бд
/// </summary>
[Authorize, Route("migration"), ApiController]
public class MigrationController(ILogger<MigrationController> logger, IDbMigrator migrator, ITokenizerService tokenizer)
    : ControllerBase
{
    /// <summary>
    /// Создать дамп бд.
    /// </summary>
    /// <param name="fileName">имя файла с дампом, либо выбор имени из ротации</param>
    [HttpGet("create")]
    public IActionResult CreateDump(string? fileName)
    {
        try
        {
            var result = migrator.Create(fileName);

            return new OkObjectResult(new { Res = Path.GetFileName(result) });
        }
        catch (Exception exception)
        {
            logger.LogError(exception, CreateError);
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
            var result = migrator.Restore(fileName);
            tokenizer.Initialize();

            return new OkObjectResult(new { Res = Path.GetFileName(result) });
        }
        catch (Exception exception)
        {
            logger.LogError(exception, RestoreError);
            return BadRequest(RestoreError);
        }
    }
}
