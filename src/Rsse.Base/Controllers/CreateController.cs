using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SearchEngine.Configuration;
using SearchEngine.Data;
using SearchEngine.Data.Dto;
using SearchEngine.Infrastructure;
using SearchEngine.Infrastructure.Tokenizer.Contracts;
using SearchEngine.Service.Models;

namespace SearchEngine.Controllers;

[Authorize]
[Route("api/create")]
[ApiController]
public class CreateController : ControllerBase
{
    private const string CreateNoteError = $"[{nameof(CreateController)}: {nameof(CreateNoteAsync)} error]";
    private const string OnGetGenreListError = $"[{nameof(CreateController)}: {nameof(OnGetGenreListAsync)} error]";

    private const string BackupFileNameConstant = "last_backup";

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
    public async Task<ActionResult<NoteDto>> OnGetGenreListAsync()
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
            return new NoteDto { ErrorMessageResponse = OnGetGenreListError };
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

            if (string.IsNullOrEmpty(result.ErrorMessageResponse))
            {
                await model.CreateTagFromTitle(dto);

                var tokenizer = scope.ServiceProvider.GetRequiredService<ITokenizerService>();

                tokenizer.Create(result.NoteId, new TextEntity { Title = dto.TitleRequest, Text = dto.TextRequest });

                // создадим дамп при выставленном флаге CreateBackupForNewSong:
                if (_baseOptions.CreateBackupForNewSong)
                {
                    // создание полного дампа достаточно ресурсозатратно:
                    _migrator.Create(BackupFileNameConstant);
                }
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, CreateNoteError);
            return new NoteDto { ErrorMessageResponse = CreateNoteError };
        }
    }
}
