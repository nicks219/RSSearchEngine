using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SearchEngine.Common.Auth;
using SearchEngine.Data.Repository.Contracts;
using SearchEngine.Engine.Contracts;
using SearchEngine.Tools.MigrationAssistant;

namespace SearchEngine.Controllers;

/// <summary>
/// Контроллер для работы с миграциями бд
/// </summary>

[Authorize, Route("migration"), ApiController]

public class MigrationController(
    ILogger<MigrationController> logger,
    IDbMigrator migrator,
    ITokenizerService tokenizer,
    // todo: MySQL WORK. DELETE
    IDataRepository repo)
    : ControllerBase
{
    private const string CreateError = $"[{nameof(MigrationController)}] {nameof(CreateDump)} error";
    private const string RestoreError = $"[{nameof(MigrationController)}] {nameof(RestoreFromDump)} error";

    private readonly ILogger<MigrationController> _logger;
    private readonly IDbMigrator _migrator;
    private readonly ITokenizerService _tokenizer;

    public MigrationController(ILogger<MigrationController> logger, IDbMigrator migrator, ITokenizerService tokenizer)
    // todo: MySQL WORK. DELETE
    [HttpGet("copy")]
    public async Task<IActionResult> CopyDatabase()
    {
        try
        {
            await repo.CopyDbFromMysqlToNpgsql();
        }
        catch (Exception exception)
        {
            const string copyError = $"[{nameof(MigrationController)}] {nameof(CopyDatabase)} error";
            logger.LogError(exception, copyError);
            return BadRequest(copyError);
        }

        return Ok("success");
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
