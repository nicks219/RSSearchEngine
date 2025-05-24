using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SearchEngine.Api.Mapping;
using SearchEngine.Data.Configuration;
using SearchEngine.Data.Dto;
using SearchEngine.Service.ApiModels;
using SearchEngine.Service.Configuration;
using SearchEngine.Service.Contracts;
using SearchEngine.Services;
using SearchEngine.Tooling.Contracts;
using static SearchEngine.Api.Messages.ControllerErrorMessages;

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
    IEnumerable<IDbMigrator> migrators,
    IOptions<CommonBaseOptions> options,
    IOptionsSnapshot<DatabaseOptions> dbOptions,
    ILogger<CreateController> logger) : ControllerBase
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
        var ct = lifetime.ApplicationStopping;
        var migratorToken = lifetime.ApplicationStopped;
        try
        {
            var noteRequestDto = request.MapToDto();

            await createService.CreateTagFromTitle(noteRequestDto, ct);
            var noteResultDto = await createService.CreateNote(noteRequestDto, ct);

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
                }, ct);

            var path = await CreateDumpAndGetFilePath(migratorToken);

            var resultDto = noteResultDto with { Text = path };

            return resultDto.MapFromDto();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, CreateNoteError);
            return new NoteResponse { ErrorMessage = CreateNoteError };
        }
    }

    /// <summary>
    /// Зафиксировать дамп бд и вернуть путь к созданному файлу.
    /// </summary>
    private async Task<string> CreateDumpAndGetFilePath(CancellationToken ct)
    {
        if (_baseOptions.CreateBackupForNewSong == false)
        {
            return string.Empty;
        }

        // Создаём дамп для читающей базы.
        var databaseType = _databaseOptions.ReaderContext;
        var migrator = IDbMigrator.GetMigrator(migrators, databaseType);

        // Будут созданы незаархивированные файлы.
        // Создание полного дампа достаточно ресурсозатратно, переходи на инкрементальные миграции.
        var path = await migrator.Create(BackupFileName, ct);
        return path;
    }
}
