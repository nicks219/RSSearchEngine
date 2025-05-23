using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SearchEngine.Data.Configuration;
using SearchEngine.Service.ApiModels;
using SearchEngine.Service.Configuration;
using SearchEngine.Service.Contracts;
using SearchEngine.Tooling.Contracts;
using SearchEngine.Tooling.MigrationAssistant;
using Swashbuckle.AspNetCore.Annotations;
using static SearchEngine.Api.Messages.ControllerErrorMessages;

namespace SearchEngine.Api.Controllers;

/// <summary>
/// Контроллер для работы с миграциями бд.
/// </summary>
[Authorize, ApiController]
[SwaggerTag("[контроллер для работы с данными]")]
public class MigrationController(
    IEnumerable<IDbMigrator> migrators,
    ITokenizerService tokenizer,
    ILogger<MigrationController> logger) : ControllerBase
{
    /// <summary>
    /// Копировать данные (включая Users) из MySql в Postgres.
    /// </summary>
    // todo: удалить после переходя на Postgres
    [HttpGet(RouteConstants.MigrationCopyGetUrl)]
    [SwaggerOperation(Summary = "копировать данные из mysql в postgres")]
    public async Task<ActionResult<StringResponse>> CopyFromMySqlToPostgres()
    {
        try
        {
            var mySqlMigrator = GetMigrator(migrators, DatabaseType.MySql);
            await mySqlMigrator.CopyDbFromMysqlToNpgsql();
            await tokenizer.Initialize();
        }
        catch (Exception exception)
        {
            var error = new StringResponse { Res = CopyError };
            logger.LogError(exception, CopyError);
            return BadRequest(error);
        }

        var response = new StringResponse { Res = "success" };
        return Ok(response);
    }

    /// <summary>
    /// Создать дамп бд.
    /// </summary>
    /// <param name="fileName">Имя файла с дампом, либо выбор имени из ротации.</param>
    /// <param name="databaseType">Тип мигратора.</param>
    [HttpGet(RouteConstants.MigrationCreateGetUrl)]
    public ActionResult<StringResponse> CreateDump(string? fileName, DatabaseType databaseType = DatabaseType.Postgres)
    {
        var migrator = GetMigrator(migrators, databaseType);

        try
        {
            var result = migrator.Create(fileName);
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
        var migrator = GetMigrator(migrators, databaseType);

        try
        {
            var result = migrator.Restore(fileName);
            await tokenizer.Initialize();
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
    public ActionResult<StringResponse> UploadFile(IFormFile file)
    {
        var path = Path.Combine(Constants.StaticDirectory, file.FileName);
        using var stream = new FileStream(path, FileMode.Create);
        file.CopyTo(stream);
        var response = $"Файл сохранён: {path}";
        return Ok(response);
    }

    /// <summary>
    /// Выгрузить файл с сервера.
    /// </summary>
    [HttpGet(RouteConstants.MigrationDownloadGetUrl)]
    [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult DownloadFile([FromQuery] string filename)
    {
        // const string mimeType = "application/octet-stream";
        const string mimeType = "application/zip";
        var path = Path.Combine(Directory.GetCurrentDirectory(), Constants.StaticDirectory, filename);
        if (!System.IO.File.Exists(path)) return NotFound("Файл не найден");

        return PhysicalFile(path, mimeType, Path.GetFileName(path));
    }

    /// <summary>
    /// Получить мигратор требуемого типа из списка зависимостей.
    /// </summary>
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

