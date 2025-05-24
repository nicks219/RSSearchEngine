using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SearchEngine.Data.Configuration;
using SearchEngine.Service.ApiModels;
using SearchEngine.Service.Configuration;
using SearchEngine.Service.Contracts;
using SearchEngine.Tooling.Contracts;
using Swashbuckle.AspNetCore.Annotations;
using static SearchEngine.Api.Messages.ControllerErrorMessages;

namespace SearchEngine.Api.Controllers;

/// <summary>
/// Контроллер для работы с миграциями бд.
/// </summary>
[Authorize, ApiController]
[SwaggerTag("[контроллер для работы с данными]")]
public class MigrationController(
    IHostApplicationLifetime lifetime,
    IEnumerable<IDbMigrator> migrators,
    ITokenizerService tokenizerService,
    ILogger<MigrationController> logger) : ControllerBase
{
    /// <summary>
    /// Копировать данные (включая Users) из MySql в Postgres.
    /// </summary>
    [HttpGet(RouteConstants.MigrationCopyGetUrl)]
    [SwaggerOperation(Summary = "копировать данные из mysql в postgres")]
    // todo: удалить после переходя на Postgres
    public async Task<ActionResult<StringResponse>> CopyFromMySqlToPostgres()
    {
        var ct = lifetime.ApplicationStopping;
        var migratorToken = lifetime.ApplicationStopped;
        try
        {
            var mySqlMigrator = IDbMigrator.GetMigrator(migrators, DatabaseType.MySql);
            await mySqlMigrator.CopyDbFromMysqlToNpgsql(migratorToken);
            await tokenizerService.Initialize(ct);
            var response = new StringResponse { Res = "success" };
            return Ok(response);
        }
        catch (Exception exception)
        {
            var error = new StringResponse { Res = CopyError };
            logger.LogError(exception, CopyError);
            return BadRequest(error);
        }
    }

    /// <summary>
    /// Создать дамп бд.
    /// </summary>
    /// <param name="fileName">Имя файла с дампом, либо выбор имени из ротации.</param>
    /// <param name="databaseType">Тип мигратора.</param>
    [HttpGet(RouteConstants.MigrationCreateGetUrl)]
    public async Task<ActionResult<StringResponse>> CreateDump(string? fileName, DatabaseType databaseType = DatabaseType.Postgres)
    {
        var migratorToken = lifetime.ApplicationStopped;
        try
        {
            var migrator = IDbMigrator.GetMigrator(migrators, databaseType);
            var result = await migrator.Create(fileName, migratorToken);
            var response = new StringResponse { Res = Path.GetFileName(result) };
            return Ok(response);
        }
        catch (Exception exception)
        {
            var error = new StringResponse { Res = CreateError };
            logger.LogError(exception, CreateError);
            return BadRequest(error);
        }
    }

    /// <summary>
    /// Накатить дамп.
    /// </summary>
    /// <param name="fileName">Имя файла с дампом, либо выбор имени из ротации.</param>
    /// <param name="databaseType">Тип мигратора.</param>
    [HttpGet(RouteConstants.MigrationRestoreGetUrl)]
    [Authorize(Constants.FullAccessPolicyName)]
    public async Task<ActionResult<StringResponse>> RestoreFromDump(string? fileName, DatabaseType databaseType = DatabaseType.Postgres)
    {
        var ct = lifetime.ApplicationStopping;
        var migratorToken = lifetime.ApplicationStopped;
        try
        {
            var migrator = IDbMigrator.GetMigrator(migrators, databaseType);
            var result = await migrator.Restore(fileName, migratorToken);
            await tokenizerService.Initialize(ct);
            var response = new StringResponse { Res = Path.GetFileName(result) };
            return Ok(response);
        }
        catch (Exception exception)
        {
            var error = new StringResponse { Res = RestoreError };
            logger.LogError(exception, RestoreError);
            return BadRequest(error);
        }
    }

    /// <summary>
    /// Загрузить файл на сервер.
    /// </summary>
    [HttpPost(RouteConstants.MigrationUploadPostUrl)]
    [RequestSizeLimit(10_000_000)]
    public async Task<ActionResult<StringResponse>> UploadFile(IFormFile file)
    {
        var ct = lifetime.ApplicationStopping;
        try
        {
            var path = Path.Combine(Constants.StaticDirectory, file.FileName);
            await using var stream = new FileStream(path, FileMode.Create);
            await file.CopyToAsync(stream, ct);
            var response = new StringResponse { Res = $"Файл сохранён: {path}" };
            return Ok(response);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, UploadError);
            var error = new StringResponse { Res = UploadError };
            return BadRequest(error);
        }
    }

    /// <summary>
    /// Выгрузить файл с сервера.
    /// </summary>
    [HttpGet(RouteConstants.MigrationDownloadGetUrl)]
    [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult DownloadFile([FromQuery] string filename)
    {
        try
        {
            // const string mimeType = "application/octet-stream";
            const string mimeType = "application/zip";
            var path = Path.Combine(Directory.GetCurrentDirectory(), Constants.StaticDirectory, filename);
            if (System.IO.File.Exists(path))
            {
                return PhysicalFile(path, mimeType, Path.GetFileName(path));
            }

            var response = new StringResponse { Res = "Файл не найден" };
            return NotFound(response);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, DownloadError);
            var error = new StringResponse { Res = DownloadError };
            return BadRequest(error);
        }
    }
}
