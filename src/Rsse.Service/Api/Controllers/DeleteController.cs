using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SearchEngine.Api.Mapping;
using SearchEngine.Service.ApiModels;
using SearchEngine.Service.Configuration;
using SearchEngine.Service.Contracts;
using SearchEngine.Services;
using static SearchEngine.Api.Messages.ControllerErrorMessages;

namespace SearchEngine.Api.Controllers;

/// <summary>
/// Контроллер для удаления сущностей.
/// </summary>
[ApiController]
[ApiExplorerSettings(IgnoreApi = !Constants.IsDebug)]
public class DeleteController(
    IHostApplicationLifetime lifetime,
    ITokenizerService tokenizerService,
    DeleteService deleteService,
    CatalogService catalogService,
    ILogger<DeleteController> logger) : ControllerBase
{
    /// <summary>
    /// Удалить заметку и вернуть обновленную страницу каталога.
    /// </summary>
    /// <param name="id">Идентификатор заметки.</param>
    /// <param name="pg">Номер страницы каталога с удаляемой заметкой.</param>
    /// <returns>Актуализированная страница каталога.</returns>
    [Authorize, HttpDelete(RouteConstants.DeleteNoteUrl)]
    [Authorize(Constants.FullAccessPolicyName)]
    public async Task<ActionResult<CatalogResponse>> DeleteNote(int id, int pg)
    {
        lifetime.ApplicationStopping.Register(() => { });
        var ct = lifetime.ApplicationStopping;
        try
        {
            await deleteService.DeleteNote(id, ct);
            await tokenizerService.Delete(id, ct);
            var catalogResultDto = await catalogService.ReadPage(pg, ct);
            return catalogResultDto.MapFromDto();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, DeleteNoteError);
            return new CatalogResponse { ErrorMessage = DeleteNoteError };
        }
    }
}
