using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SearchEngine.Api.Mapping;
using SearchEngine.Domain.ApiModels;
using SearchEngine.Domain.Configuration;
using SearchEngine.Domain.Contracts;
using SearchEngine.Domain.Services;
using static SearchEngine.Domain.Configuration.ControllerMessages;

namespace SearchEngine.Api.Controllers;

/// <summary>
/// Контроллер для удаления сущностей.
/// </summary>
[ApiController]
[ApiExplorerSettings(IgnoreApi = !Constants.IsDebug)]
public class DeleteController(
    ITokenizerService tokenizer,
    DeleteService deleteService,
    ILogger<DeleteController> logger) : ControllerBase
{
    /// <summary>
    /// Удалить заметку.
    /// </summary>
    /// <param name="id">Идентификатор заметки.</param>
    /// <param name="pg">Номер страницы каталога с удаляемой заметкой.</param>
    /// <returns>Актуализированная страница каталога.</returns>
    [Authorize, HttpDelete(RouteConstants.DeleteNoteUrl)]
    [Authorize(Constants.FullAccessPolicyName)]
    public async Task<ActionResult<CatalogResponse>> DeleteNote(int id, int pg)
    {
        try
        {
            var response = await deleteService.DeleteNote(id, pg);
            await tokenizer.Delete(id);
            return response.MapFromDto();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, DeleteNoteError);
            return new CatalogResponse { ErrorMessage = DeleteNoteError };
        }
    }
}
