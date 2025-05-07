using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SearchEngine.Api.Mapping;
using SearchEngine.Domain.ApiModels;
using SearchEngine.Domain.Configuration;
using SearchEngine.Domain.Contracts;
using SearchEngine.Domain.Entities;
using SearchEngine.Domain.Managers;
using static SearchEngine.Domain.Configuration.ControllerMessages;

namespace SearchEngine.Api.Controllers;

/// <summary>
/// Контроллер для обновления заметки
/// </summary>
[Authorize, ApiController]
[ApiExplorerSettings(IgnoreApi = !Constants.IsDebug)]
public class UpdateController(
    IDataRepository repo,
    ITokenizerService tokenizer,
    ILoggerFactory loggerFactory) : ControllerBase
{
    private readonly ILogger<UpdateController> _logger = loggerFactory.CreateLogger<UpdateController>();
    private readonly ILogger<UpdateManager> _managerLogger = loggerFactory.CreateLogger<UpdateManager>();

    /// <summary>
    /// Обновить заметку
    /// </summary>
    /// <param name="request">данные для обновления</param>
    [Authorize, HttpPut(RouteConstants.UpdateNotePutUrl)]
    public async Task<ActionResult<NoteResponse>> UpdateNote([FromBody] NoteRequest request)
    {
        try
        {
            var dto = request.MapToDto();
            var response = await new UpdateManager(repo, _managerLogger).UpdateNote(dto);

            tokenizer.Update(dto.NoteIdExchange, new NoteEntity { Title = dto.TitleRequest, Text = dto.TextRequest });

            return response.MapFromDto();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, UpdateNoteError);
            return new NoteResponse { CommonErrorMessageResponse = UpdateNoteError };
        }
    }
}
