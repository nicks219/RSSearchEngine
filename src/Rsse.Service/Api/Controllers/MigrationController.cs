using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SearchEngine.Domain.Configuration;
using SearchEngine.Domain.Contracts;
using SearchEngine.Tooling.Contracts;
using SearchEngine.Tooling.MigrationAssistant;
using Swashbuckle.AspNetCore.Annotations;
using static SearchEngine.Domain.Configuration.ControllerMessages;

namespace SearchEngine.Api.Controllers;

/// <summary>
/// Контроллер для работы с миграциями бд
/// </summary>
[Authorize, ApiController]
[SwaggerTag("[контроллер для работы с данными]")]
public class MigrationController(
    IEnumerable<IDbMigrator> migrators,
    ITokenizerService tokenizer,
    IDataRepository repo,
    ILoggerFactory loggerFactory) : ControllerBase
{
    private readonly ILogger<MigrationController> _logger = loggerFactory.CreateLogger<MigrationController>();

    /// <summary>
    /// Копировать данные (включая Users) из MySql в Postgres.
    /// </summary>
    /// <returns></returns>
    // todo: MySQL WORK. DELETE
    [HttpGet(RouteConstants.MigrationCopyGetUrl)]
    [SwaggerOperation(Summary = "копировать данные из mysql в postgres")]
    public async Task<IActionResult> CopyFromMySqlToPostgres()
    {
        try
        {
            await repo.CopyDbFromMysqlToNpgsql();
            await tokenizer.Initialize();
        }
        catch (Exception exception)
        {
            const string copyError = $"[{nameof(MigrationController)}] {nameof(CopyFromMySqlToPostgres)} error";
            _logger.LogError(exception, copyError);
            return BadRequest(copyError);
        }

        return new OkObjectResult(new { Res = "success" });
    }

    /// <summary>
    /// Создать дамп бд.
    /// </summary>
    /// <param name="fileName">имя файла с дампом, либо выбор имени из ротации</param>
    /// <param name="databaseType">тип мигратора</param>
    [HttpGet(RouteConstants.MigrationCreateGetUrl)]
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
            _logger.LogError(exception, CreateError);
            return BadRequest(CreateError);
        }
    }

    /// <summary>
    /// Накатить дамп.
    /// </summary>
    /// <param name="fileName">имя файла с дампом, либо выбор имени из ротации</param>
    /// <param name="databaseType">тип мигратора</param>
    [HttpGet(RouteConstants.MigrationRestoreGetUrl)]
    [Authorize(Constants.FullAccessPolicyName)]
    public async Task<IActionResult> RestoreFromDump(string? fileName, DatabaseType databaseType = DatabaseType.Postgres)
    {
        var migrator = GetMigrator(migrators, databaseType);

        try
        {
            var result = migrator.Restore(fileName);
            await tokenizer.Initialize();

            return new OkObjectResult(new { Res = Path.GetFileName(result) });
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, RestoreError);
            return BadRequest(RestoreError);
        }
    }

    /// <summary>
    /// Загрузить файл на сервер
    /// </summary>
    [HttpPost(RouteConstants.MigrationUploadPostUrl)]
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
    [HttpGet(RouteConstants.MigrationDownloadGetUrl)]
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

