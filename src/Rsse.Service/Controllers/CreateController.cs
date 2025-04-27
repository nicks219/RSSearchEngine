using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SearchEngine.Common.Configuration;
using SearchEngine.Data.Dto;
using SearchEngine.Data.Entities;
using SearchEngine.Engine.Contracts;
using SearchEngine.Models;
using SearchEngine.Tools.MigrationAssistant;
using static SearchEngine.Common.ControllerMessages;

namespace SearchEngine.Controllers;

/// <summary>
/// Контроллер для создания заметок
/// </summary>
[Authorize, Route("api/create"), ApiController]
[ApiExplorerSettings(IgnoreApi = !Common.Auth.Constants.IsDebug)]
public class CreateController(
    IServiceScopeFactory serviceScopeFactory,
    ILogger<CreateController> logger,
    IDbMigrator migrator,
    IOptions<CommonBaseOptions> options)
    : ControllerBase
{
    private const string BackupFileName = "db_last_dump";

    private readonly CommonBaseOptions _baseOptions = options.Value;

    /// <summary>
    /// Получить список тегов
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<NoteDto>> GetStructuredTagListAsync()
    {
        try
        {
            using var scope = serviceScopeFactory.CreateScope();
            var model = new CreateModel(scope);
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
            using var scope = serviceScopeFactory.CreateScope();
            var model = new CreateModel(scope);

            var result = await model.CreateNote(dto);

            if (!string.IsNullOrEmpty(result.CommonErrorMessageResponse))
            {
                return result;
            }

            await model.CreateTagFromTitle(dto);

            var tokenizer = scope.ServiceProvider.GetRequiredService<ITokenizerService>();

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
        return _baseOptions.CreateBackupForNewSong
            // NB: создание полного дампа достаточно ресурсозатратно, переходи на инкрементальные минрации:
            ? migrator.Create(BackupFileName)
            : null;
    }
}
