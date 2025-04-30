using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SearchEngine.Common;
using SearchEngine.Common.Auth;
using SearchEngine.Data.Repository;
using SearchEngine.Data.Repository.Contracts;
using SearchEngine.Engine.Contracts;
using SearchEngine.Tools.MigrationAssistant;
using static SearchEngine.Common.ControllerMessages;
using Swashbuckle.AspNetCore.Annotations;

namespace SearchEngine.Controllers;

/// <summary>
/// Контроллер для работы с миграциями бд
/// </summary>
[Authorize, Route("migration"), ApiController]
[SwaggerTag("[контроллер для работы с данными]")]
public class MigrationController(
    ILogger<MigrationController> logger,
    IEnumerable<IDbMigrator> migrators,
    ITokenizerService tokenizer,
    IDataRepository repo) : ControllerBase
{
    /// <summary>
    /// Копировать данные (включая Users) из MySql в Postgres.
    /// </summary>
    /// <returns></returns>
    // todo: MySQL WORK. DELETE
    [HttpGet("copy")]
    [SwaggerOperation(Summary = "копировать данные из mysql в postgres")]
    public async Task<IActionResult> CopyFromMySqlToPostgres()
    {
        try
        {
            await repo.CopyDbFromMysqlToNpgsql();
            tokenizer.Initialize();
        }
        catch (Exception exception)
        {
            const string copyError = $"[{nameof(MigrationController)}] {nameof(CopyFromMySqlToPostgres)} error";
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
    public IActionResult CreateDump(string? fileName, DatabaseType databaseType = DatabaseType.Postgres)
    {
        var migrator = GetMigrator(migrators, databaseType);

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
    public IActionResult RestoreFromDump(string? fileName, DatabaseType databaseType = DatabaseType.Postgres)
    {
        var migrator = GetMigrator(migrators, databaseType);

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

    /// <summary>
    /// Загрузить файл на сервер
    /// </summary>
    [HttpPost("upload")]
    [RequestSizeLimit(10_000_000)]
    public IActionResult UploadFile(IFormFile file)
    {
        var path = Path.Combine(Constants.StaticDirectory, file.FileName);
        using var stream = new FileStream(path, FileMode.Create);
        file.CopyTo(stream);
        return Ok($"Файл сохранён: {path}");
    }

    /// <summary>
    /// Выгрузить файл с сервера
    /// </summary>
    [HttpGet("download")]
    [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult DownloadFile([FromQuery] string filename)
    {
        // const string mimeType = "application/octet-stream";
        const string mimeType = "application/zip";
        var path = Path.Combine(Directory.GetCurrentDirectory(), Constants.StaticDirectory, filename);
        if (!System.IO.File.Exists(path)) return NotFound("Файл не найден");

        return PhysicalFile(path, mimeType, Path.GetFileName(path));
    }

    internal static IDbMigrator GetMigrator(IEnumerable<IDbMigrator> migrators, DatabaseType databaseType)
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

