using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using SearchEngine.Api.Mapping;
using SearchEngine.Data.Configuration;
using SearchEngine.Data.Dto;
using SearchEngine.Service.ApiModels;
using SearchEngine.Service.Configuration;
using SearchEngine.Service.Contracts;
using SearchEngine.Services;
using SearchEngine.Tooling.Contracts;

namespace SearchEngine.Api.Controllers;

/// <summary>
/// Контроллер для создания заметок.
/// </summary>
[Authorize, ApiController]
[ApiExplorerSettings(IgnoreApi = !Constants.IsDebug)]
public class CreateController(
    IHostApplicationLifetime lifetime,
    ITokenizerService tokenizerService,
    CreateService createService,
    IDbMigratorFactory migratorFactory,
    IOptions<CommonBaseOptions> options,
    IOptionsSnapshot<DatabaseOptions> dbOptions) : ControllerBase
{

    private const string BackupFileName = "db_last_dump";
    private readonly CommonBaseOptions _baseOptions = options.Value;
    private readonly DatabaseOptions _databaseOptions = dbOptions.Value;

    /// <summary>
    /// Создать заметку.
    /// </summary>
    /// <param name="request">Контрейнер с запросом создания заметки.</param>
    /// <returns>Контейнер с информацией по созданной заметке, либо с ошибкой.</returns>
    [HttpPost(RouteConstants.CreateNotePostUrl)]
    public async Task<ActionResult<NoteResponse>> CreateNoteAndDumpAsync([FromBody] NoteRequest request)
    {
        var stoppingToken = lifetime.ApplicationStopping;
        if (stoppingToken.IsCancellationRequested) return StatusCode(503);

        var noteRequestDto = request.MapToDto();

        await createService.CreateTagFromTitle(noteRequestDto, stoppingToken);
        var noteResultDto = await createService.CreateNote(noteRequestDto, stoppingToken);

        if (!string.IsNullOrEmpty(noteResultDto.ErrorMessage))
        {
            return new NoteResponse
            {
                Title = noteResultDto.Title,
                Text = noteResultDto.Text,
                StructuredTags = noteResultDto.EnrichedTags,
                ErrorMessage = noteResultDto.ErrorMessage
            };
        }

        await tokenizerService.Create(
            noteResultDto.NoteIdExchange,
            new TextRequestDto
            {
                Title = request.Title,
                Text = request.Text
            }, stoppingToken);

        var path = await CreateDumpAndGetFilePath(stoppingToken);

        var resultDto = noteResultDto with { Text = path };

        var noteResponse = resultDto.MapFromDto();

        return Ok(noteResponse);
    }

    /// <summary>
    /// Зафиксировать дамп бд и вернуть путь к созданному файлу.
    /// </summary>
    private async Task<string> CreateDumpAndGetFilePath(CancellationToken stoppingToken)
    {
        if (_baseOptions.CreateBackupForNewSong == false)
        {
            return string.Empty;
        }

        // Создаём дамп для читающей базы.
        var databaseType = _databaseOptions.ReaderContext;
        var migrator = migratorFactory.CreateMigrator(databaseType);

        // Будут созданы незаархивированные файлы.
        // Создание полного дампа достаточно ресурсозатратно, переходи на инкрементальные миграции.
        var path = await migrator.Create(BackupFileName, stoppingToken);
        return path;
    }
}
