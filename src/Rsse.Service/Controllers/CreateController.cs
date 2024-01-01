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
using SearchEngine.Tools.Migrator;

namespace SearchEngine.Controllers;

/// <summary>
/// Контроллер для создания заметок
/// </summary>

[Authorize, Route("api/create"), ApiController]

public class CreateController : ControllerBase
{
    private const string CreateNoteError = $"[{nameof(CreateController)}] {nameof(CreateNoteAndDumpAsync)} error";
    private const string GetTagListError = $"[{nameof(CreateController)}] {nameof(GetStructuredTagListAsync)} error";

    private const string BackupFileName = "db_last_dump";

    private readonly ILogger<CreateController> _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly IDbMigrator _migrator;
    private readonly CommonBaseOptions _baseOptions;

    public CreateController(
        IServiceScopeFactory serviceScopeFactory,
        ILogger<CreateController> logger,
        IDbMigrator migrator,
        IOptions<CommonBaseOptions> options)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
        _migrator = migrator;
        _baseOptions = options.Value;
    }

    /// <summary>
    /// Получить список тегов
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<NoteDto>> GetStructuredTagListAsync()
    {
        try
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var model = new CreateModel(scope);
            return await model.ReadStructuredTagList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, GetTagListError);
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
            using var scope = _serviceScopeFactory.CreateScope();
            var model = new CreateModel(scope);

            var result = await model.CreateNote(dto);

            if (!string.IsNullOrEmpty(result.CommonErrorMessageResponse))
            {
                return result;
            }

            await model.CreateTagFromTitle(dto);

            var tokenizer = scope.ServiceProvider.GetRequiredService<ITokenizerService>();

            tokenizer.Create(result.CommonNoteId, new NoteEntity { Title = dto.TitleRequest, Text = dto.TextRequest });

            CreateDump();

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, CreateNoteError);
            return new NoteDto { CommonErrorMessageResponse = CreateNoteError };
        }
    }

    /// <summary>
    /// Зафиксировать дамп бд
    /// </summary>
    private void CreateDump()
    {
        if (_baseOptions.CreateBackupForNewSong)
        {
            // создание полного дампа достаточно ресурсозатратно:
            _migrator.Create(BackupFileName);
        }
    }
}
