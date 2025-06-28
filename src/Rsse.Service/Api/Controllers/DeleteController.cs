using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using SearchEngine.Service.Api;
using SearchEngine.Service.ApiModels;
using SearchEngine.Service.Configuration;
using SearchEngine.Service.Contracts;
using SearchEngine.Service.Mapping;

namespace SearchEngine.Api.Controllers;

/// <summary>
/// Контроллер для удаления сущностей.
/// </summary>
[ApiController]
[ApiExplorerSettings(IgnoreApi = !Constants.IsDebug)]
public class DeleteController(
    IHostApplicationLifetime lifetime,
    ITokenizerApiClient tokenizerApiClient,
    DeleteService deleteService,
    CatalogService catalogService) : ControllerBase
{
    /// <summary>
    /// Удалить заметку и вернуть обновленную страницу каталога.
    /// </summary>
    /// <param name="id">Идентификатор заметки.</param>
    /// <param name="pg">Номер страницы каталога с удаляемой заметкой.</param>
    /// <returns>Актуализированная страница каталога.</returns>
    [Authorize, HttpDelete(RouteConstants.DeleteNoteUrl)]
    [Authorize(Constants.FullAccessPolicyName)]
    public async Task<ActionResult<CatalogResponse>> DeleteNote(
        [FromQuery][Required(AllowEmptyStrings = false)] int id,
        [FromQuery][Required(AllowEmptyStrings = false)] int pg)
    {
        var stoppingToken = lifetime.ApplicationStopping;
        if (stoppingToken.IsCancellationRequested) return StatusCode(503);

        await deleteService.DeleteNote(id, stoppingToken);
        await tokenizerApiClient.Delete(id, stoppingToken);
        var catalogResultDto = await catalogService.ReadPage(pg, stoppingToken);
        var catalogResponse = catalogResultDto.MapFromDto();
        return Ok(catalogResponse);
    }
}
