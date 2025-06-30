using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using SearchEngine.Api.Services;
using SearchEngine.Api.Startup;
using SearchEngine.Data.Configuration;
using SearchEngine.Service.ApiModels;
using SearchEngine.Service.Configuration;
using SearchEngine.Service.Contracts;
using SearchEngine.Tooling.Contracts;
using Serilog;
using Swashbuckle.AspNetCore.Annotations;

namespace SearchEngine.Api.Controllers;

/// <summary>
/// Контроллер для работы с миграциями бд.
/// </summary>
[Authorize, ApiController]
[SwaggerTag("[контроллер для работы с данными]")]
public class MigrationController(
    DbDataProvider dbDataProvider,
    IHostApplicationLifetime lifetime,
    IDbMigratorFactory migratorFactory,
    ITokenizerApiClient tokenizerApiClient) : ControllerBase
{
    /// <summary>
    /// Залогировать тестовое сообщение с уровнем warning и отдать метрику.
    /// </summary>
    [HttpGet("/observabilty/report")]
    public ActionResult ReportTestDataWithMetrics()
    {
        var activity = Activity.Current;
        activity?.AddEvent(new ActivityEvent("trace-log-event"));
        MetricsExtensions.HistogramWithExemplar.Record(1D);
        Log.Warning(
            $"{nameof(ReportTestDataWithMetrics)} | produce warning log with activity and histogram for testing purposes only.");
        return Ok("Span, log and histogram created via ActivityEvent");
    }

    /// <summary>
    /// Залогировать тестовое сообщение с уровнем warning.
    /// </summary>
    [HttpGet("/observabilty/report-without-metrcis")]
    public ActionResult ReportTestDataWithoutMetrics()
    {
        var activity = Activity.Current;
        activity?.AddEvent(new ActivityEvent("trace-log-event"));
        Log.Warning(
            $"{nameof(ReportTestDataWithoutMetrics)} | produce warning log with activity for testing purposes only.");
        return Ok("Span and log created via ActivityEvent");
    }

    /// <summary>
    /// Копировать данные (включая Users) из MySql в Postgres.
    /// </summary>
    [HttpGet(RouteConstants.MigrationCopyGetUrl)]
    [SwaggerOperation(Summary = "копировать данные из mysql в postgres")]
    // todo: удалить после переходя на Postgres
    public async Task<ActionResult<StringResponse>> CopyFromMySqlToPostgres()
    {
        var stoppingToken = lifetime.ApplicationStopping;
        if (stoppingToken.IsCancellationRequested) return StatusCode(503);

        var mySqlMigrator = migratorFactory.CreateMigrator(DatabaseType.MySql);
        await mySqlMigrator.CopyDbFromMysqlToNpgsql(stoppingToken);
        await tokenizerApiClient.Initialize(dbDataProvider, stoppingToken);
        var response = new StringResponse { Res = "success" };
        return Ok(response);
    }

    /// <summary>
    /// Создать дамп бд.
    /// </summary>
    /// <param name="fileName">Имя файла с дампом, либо выбор имени из ротации.</param>
    /// <param name="databaseType">Тип мигратора.</param>
    [HttpGet(RouteConstants.MigrationCreateGetUrl)]
    public async Task<ActionResult<StringResponse>> CreateDump(
        [FromQuery] string? fileName,
        DatabaseType databaseType = DatabaseType.Postgres)
    {
        var stoppingToken = lifetime.ApplicationStopping;
        if (stoppingToken.IsCancellationRequested) return StatusCode(503);

        var migrator = migratorFactory.CreateMigrator(databaseType);
        var result = await migrator.Create(fileName, stoppingToken);
        var response = new StringResponse { Res = Path.GetFileName(result) };
        return Ok(response);
    }

    /// <summary>
    /// Накатить дамп.
    /// </summary>
    /// <param name="fileName">Имя файла с дампом, либо выбор имени из ротации.</param>
    /// <param name="databaseType">Тип мигратора.</param>
    [HttpGet(RouteConstants.MigrationRestoreGetUrl)]
    [Authorize(Constants.FullAccessPolicyName)]
    public async Task<ActionResult<StringResponse>> RestoreFromDump(
        [FromQuery] string? fileName,
        DatabaseType databaseType = DatabaseType.Postgres)
    {
        var stoppingToken = lifetime.ApplicationStopping;
        if (stoppingToken.IsCancellationRequested) return StatusCode(503);

        var migrator = migratorFactory.CreateMigrator(databaseType);
        var result = await migrator.Restore(fileName, stoppingToken);
        await tokenizerApiClient.Initialize(dbDataProvider, stoppingToken);
        var response = new StringResponse { Res = Path.GetFileName(result) };
        return Ok(response);
    }

    /// <summary>
    /// Загрузить файл на сервер.
    /// </summary>
    [HttpPost(RouteConstants.MigrationUploadPostUrl)]
    [RequestSizeLimit(10_000_000)]
    // [FromForm] см https://github.com/domaindrivendev/Swashbuckle.AspNetCore/blob/master/docs/configure-and-customize-swaggergen.md#handle-forms-and-file-uploads
    public async Task<ActionResult<StringResponse>> UploadFile(
        [Required(AllowEmptyStrings = false)] IFormFile file)
    {
        var stoppingToken = lifetime.ApplicationStopping;
        if (stoppingToken.IsCancellationRequested) return StatusCode(503);

        var path = Path.Combine(Constants.StaticDirectory, file.FileName);
        await using var stream = new FileStream(path, FileMode.Create);
        await file.CopyToAsync(stream, stoppingToken);
        var response = new StringResponse { Res = $"Файл сохранён: {path}" };
        return Ok(response);
    }

    /// <summary>
    /// Выгрузить файл с сервера.
    /// </summary>
    [HttpGet(RouteConstants.MigrationDownloadGetUrl)]
    [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult DownloadFile(
        [FromQuery][Required(AllowEmptyStrings = false)] string filename,
        CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested) return StatusCode(503);

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
}
