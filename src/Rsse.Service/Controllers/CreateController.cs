using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SearchEngine.Configuration;
using SearchEngine.Data.Dto;
using SearchEngine.Data.Entities;
using SearchEngine.Infrastructure;
using SearchEngine.Infrastructure.Tokenizer.Contracts;
using SearchEngine.Service.Models;

namespace SearchEngine.Controllers;

[Authorize]
[Route("api/create")]
[ApiController]
public class CreateController : ControllerBase
{
    private const string CreateNoteError = $"[{nameof(CreateController)}] {nameof(CreateNoteAsync)} error";
    private const string OnGetGenreListError = $"[{nameof(CreateController)}] {nameof(GetTagListAsync)} error";

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

    [HttpGet]
    public async Task<ActionResult<NoteDto>> GetTagListAsync()
    {
        try
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var model = new CreateModel(scope);
            return await model.ReadTagList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, OnGetGenreListError);
            return new NoteDto { CommonErrorMessageResponse = OnGetGenreListError };
        }
    }

    [HttpPost]
    public async Task<ActionResult<NoteDto>> CreateNoteAsync([FromBody] NoteDto dto)
    {
        try
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var model = new CreateModel(scope);

            var result = await model.CreateNote(dto);

            if (string.IsNullOrEmpty(result.CommonErrorMessageResponse))
            {
                await model.CreateTagFromTitle(dto);

                var tokenizer = scope.ServiceProvider.GetRequiredService<ITokenizerService>();

                tokenizer.Create(result.CommonNoteId, new NoteEntity { Title = dto.TitleRequest, Text = dto.TextRequest });

                // создадим дамп при выставленном флаге CreateBackupForNewSong:
                if (_baseOptions.CreateBackupForNewSong)
                {
                    // создание полного дампа достаточно ресурсозатратно:
                    _migrator.Create(BackupFileName);
                }
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, CreateNoteError);
            return new NoteDto { CommonErrorMessageResponse = CreateNoteError };
        }
    }
}
