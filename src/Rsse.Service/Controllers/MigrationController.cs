using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SearchEngine.Common.Auth;
using SearchEngine.Data.Repository;
using SearchEngine.Data.Repository.Contracts;
using SearchEngine.Engine.Contracts;
using SearchEngine.Tools.MigrationAssistant;
using static SearchEngine.Common.ControllerMessages;

namespace SearchEngine.Controllers;

/// <summary>
/// Контроллер для работы с миграциями бд
/// </summary>
[Authorize, Route("migration"), ApiController]
public class MigrationController(
    ILogger<MigrationController> logger,
    IEnumerable<IDbMigrator> migrators,
    ITokenizerService tokenizer,
    // todo: MySQL WORK. DELETE
    IDataRepository repo) : ControllerBase
{
    /// <summary>
    /// Копировать данные (включая Users) из MySql в Postgres.
    /// </summary>
    /// <returns></returns>
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
    /// <param name="databaseType">тип мигратора</param>
    [HttpGet("create")]
    public IActionResult CreateDump(string? fileName, DatabaseType databaseType = DatabaseType.MySql)
    {
        var migrator = GetMigrator(databaseType);

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
    /// <param name="databaseType">тип мигратора</param>
    [HttpGet("restore")]
    [Authorize(Constants.FullAccessPolicyName)]
    public IActionResult RestoreFromDump(string? fileName, DatabaseType databaseType = DatabaseType.MySql)
    {
        var migrator = GetMigrator(databaseType);

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

    private IDbMigrator GetMigrator(DatabaseType databaseType)
    {
        var migrator = databaseType switch
        {
            DatabaseType.MySql => migrators.First(m => m.GetType() == typeof(MySqlDbMigrator)),
            DatabaseType.Postgres => migrators.First(m => m.GetType() == typeof(NpgsqlDbMigrator)),
            _ => throw new ArgumentOutOfRangeException(nameof(databaseType), databaseType, "unknown database type")
        };

        return migrator;
    }
}

