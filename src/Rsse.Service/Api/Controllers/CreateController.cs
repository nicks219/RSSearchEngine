using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SearchEngine.Domain.Configuration;
using SearchEngine.Domain.Contracts;
using SearchEngine.Domain.Dto;
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
    public async Task<ActionResult<NoteDto>> GetStructuredTagListAsync()
    {
        try
        {
            var scopedProvider = HttpContext.RequestServices;
            var model = new CreateManager(scopedProvider);
            return await model.ReadStructuredTagList();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, GetTagListError);
            return new NoteDto { CommonErrorMessageResponse = GetTagListError };
        }
    }

    /// <summary>
    /// Создать заметку
    /// </summary>
    /// <param name="dto">данные для создания заметки</param>
    /// <returns>данные с созданной заметкой либо ошибкой</returns>
    [HttpPost]
    public async Task<ActionResult<NoteDto>> CreateNoteAndDumpAsync([FromBody] NoteDto dto)
    {
        try
        {
            var scopedProvider = HttpContext.RequestServices;
            var model = new CreateManager(scopedProvider);

            await model.CreateTagFromTitle(dto);
            var result = await model.CreateNote(dto);

            if (!string.IsNullOrEmpty(result.CommonErrorMessageResponse))
            {
                return result;
            }

            // await model.CreateTagFromTitle(dto); // перенесен до создания заметки

            var tokenizer = scopedProvider.GetRequiredService<ITokenizerService>();

            tokenizer.Create(result.CommonNoteId, new NoteEntity { Title = dto.TitleRequest, Text = dto.TextRequest });

            var path = CreateDumpAndGetFilePath();

            result.TextResponse = path ?? string.Empty;

            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, CreateNoteError);
            return new NoteDto { CommonErrorMessageResponse = CreateNoteError };
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
