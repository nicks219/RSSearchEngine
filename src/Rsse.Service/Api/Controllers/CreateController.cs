using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
    ITokenizerService tokenizer,
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
    /// <returns>Контрейнер с информацией по созданной заметке, либо с ошибкой.</returns>
    [HttpPost(RouteConstants.CreateNotePostUrl)]
    public async Task<ActionResult<NoteResponse>> CreateNoteAndDumpAsync([FromBody] NoteRequest request)
    {
        try
        {
            var noteRequestDto = request.MapToDto();

            await createService.CreateTagFromTitle(noteRequestDto);
            var noteResultDto = await createService.CreateNote(noteRequestDto);

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

            await tokenizer.Create(
                noteResultDto.NoteIdExchange,
                new TextRequestDto
                {
                    Title = request.Title,
                    Text = request.Text
                });

            var path = CreateDumpAndGetFilePath();

            // todo: можно добавить в маппер
            noteResultDto = noteResultDto with
            {
                Text = path ?? string.Empty
            };

            return noteResultDto.MapFromDto();
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
    private string? CreateDumpAndGetFilePath()
    {
        // NB: создадим дамп для читающей базы
        var dbType = _databaseOptions.ReaderContext;
        var migrator = MigrationController.GetMigrator(migrators, dbType);

        // NB: будут созданы незаархивированные файлы
        return _baseOptions.CreateBackupForNewSong
            // NB: создание полного дампа достаточно ресурсозатратно, переходи на инкрементальные минрации:
            ? migrator.Create(BackupFileName)
            : null;
    }
}
