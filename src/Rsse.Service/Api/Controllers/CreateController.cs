using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SearchEngine.Api.Mapping;
using SearchEngine.Domain.ApiModels;
using SearchEngine.Domain.Configuration;
using SearchEngine.Domain.Contracts;
using SearchEngine.Domain.Entities;
using SearchEngine.Domain.Managers;
using SearchEngine.Tooling.Contracts;
using static SearchEngine.Domain.Configuration.ControllerMessages;

namespace SearchEngine.Api.Controllers;

/// <summary>
/// Контроллер для создания заметок
/// </summary>
[Authorize, ApiController]
[ApiExplorerSettings(IgnoreApi = !Constants.IsDebug)]
public class CreateController(
    ITokenizerService tokenizer,
    CreateManager manager,
    IEnumerable<IDbMigrator> migrators,
    IOptions<CommonBaseOptions> options,
    IOptionsSnapshot<DatabaseOptions> dbOptions,
    ILogger<CreateController> logger) : ControllerBase
{

    private const string BackupFileName = "db_last_dump";
    private readonly CommonBaseOptions _baseOptions = options.Value;
    private readonly DatabaseOptions _databaseOptions = dbOptions.Value;

    /// <summary>
    /// Создать заметку
    /// </summary>
    /// <param name="request">данные для создания заметки</param>
    /// <returns>данные с созданной заметкой либо ошибкой</returns>
    [HttpPost(RouteConstants.CreateNotePostUrl)]
    public async Task<ActionResult<NoteResponse>> CreateNoteAndDumpAsync([FromBody] NoteRequest request)
    {
        try
        {
            var noteRequestDto = request.MapToDto();

            await manager.CreateTagFromTitle(noteRequestDto);
            var noteResultDto = await manager.CreateNote(noteRequestDto);

            if (!string.IsNullOrEmpty(noteResultDto.CommonErrorMessageResponse))
            {
                return new NoteResponse
                {
                    TitleResponse = noteResultDto.TitleResponse,
                    TextResponse = noteResultDto.TextResponse,
                    StructuredTagsListResponse = noteResultDto.StructuredTagsListResponse,
                    CommonErrorMessageResponse = noteResultDto.CommonErrorMessageResponse
                };
            }

            await tokenizer.Create(
                noteResultDto.NoteIdExchange,
                new NoteEntity
                {
                    Title = request.TitleRequest,
                    Text = request.TextRequest
                });

            var path = CreateDumpAndGetFilePath();

            // todo: можно добавить в маппер
            noteResultDto = noteResultDto with
            {
                TextResponse = path ?? string.Empty
            };

            return noteResultDto.MapFromDto();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, CreateNoteError);
            return new NoteResponse { CommonErrorMessageResponse = CreateNoteError };
        }
    }

    /// <summary>
    /// Зафиксировать дамп бд и вернуть путь к созданному файлу
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
