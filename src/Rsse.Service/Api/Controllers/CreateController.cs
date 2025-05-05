using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
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
[Authorize, Route("api/create"), ApiController]
[ApiExplorerSettings(IgnoreApi = !Constants.IsDebug)]
public class CreateController(
    ILogger<CreateController> logger,
    IEnumerable<IDbMigrator> migrators,
    IOptions<CommonBaseOptions> options,
    IOptionsSnapshot<DatabaseOptions> dbOptions)
    : ControllerBase
{
    private const string BackupFileName = "db_last_dump";
    private readonly CommonBaseOptions _baseOptions = options.Value;
    private readonly DatabaseOptions _databaseOptions = dbOptions.Value;

    /// <summary>
    /// Получить список тегов
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<NoteResponse>> GetStructuredTagListAsync()
    {
        try
        {
            var scopedProvider = HttpContext.RequestServices;
            var repo = scopedProvider.GetRequiredService<IDataRepository>();
            var managerLogger = scopedProvider.GetRequiredService<ILogger<CreateManager>>();

            var model = new CreateManager(repo, managerLogger);
            var response = await model.ReadStructuredTagList();
            return response.MapFromDto();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, GetTagListError);
            return new NoteResponse { CommonErrorMessageResponse = GetTagListError };
        }
    }

    /// <summary>
    /// Создать заметку
    /// </summary>
    /// <param name="request">данные для создания заметки</param>
    /// <returns>данные с созданной заметкой либо ошибкой</returns>
    [HttpPost]
    public async Task<ActionResult<NoteResponse>> CreateNoteAndDumpAsync([FromBody] NoteRequest request)
    {
        try
        {
            var scopedProvider = HttpContext.RequestServices;
            var repo = scopedProvider.GetRequiredService<IDataRepository>();
            var managerLogger = scopedProvider.GetRequiredService<ILogger<CreateManager>>();

            var manager = new CreateManager(repo, managerLogger);
            var dto = request.MapToDto();

            await manager.CreateTagFromTitle(dto);
            var response = await manager.CreateNote(dto);

            if (!string.IsNullOrEmpty(response.CommonErrorMessageResponse))
            {
                // теги structuredTagsListResponse - ошибка - титл - текст
                return new NoteResponse
                {
                    // NoteExchangeId ??
                    TitleResponse = response.TitleResponse,
                    TextResponse = response.TextResponse,
                    StructuredTagsListResponse = response.StructuredTagsListResponse,
                    CommonErrorMessageResponse = response.CommonErrorMessageResponse
                };
            }

            // await model.CreateTagFromTitle(dto); // перенесен до создания заметки

            var tokenizer = scopedProvider.GetRequiredService<ITokenizerService>();

            tokenizer.Create(response.NoteIdExchange, new NoteEntity { Title = request.TitleRequest, Text = request.TextRequest });

            var path = CreateDumpAndGetFilePath();

            response.TextResponse = path ?? string.Empty;

            return response.MapFromDto();
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
