using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SearchEngine.Service.ApiModels;
using SearchEngine.Service.Configuration;
using SearchEngine.Service.Mapping;
using SearchEngine.Services;

namespace SearchEngine.Api.Controllers;

/// <summary>
/// Контроллер для функционала каталога.
/// </summary>
[ApiController]
[ApiExplorerSettings(IgnoreApi = !Constants.IsDebug)]
public class CatalogController(CatalogService catalogService) : ControllerBase
{
    /// <summary>
    /// Прочитать страницу каталога.
    /// </summary>
    /// <param name="id">Номер страницы.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    [HttpGet(RouteConstants.CatalogPageGetUrl)]
    public async Task<ActionResult<CatalogResponse>> ReadCatalogPage(
        [FromQuery][Required(AllowEmptyStrings = false)] int id,
        CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested) return StatusCode(503);

        var response = await catalogService.ReadPage(id, cancellationToken);
        var catalogResponse = response.MapFromDto();
        return Ok(catalogResponse);
    }

    /// <summary>
    /// Переместиться по каталогу.
    /// </summary>
    /// <param name="request">Контейнер с информацией для навигации по каталогу.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    [HttpPost(RouteConstants.CatalogNavigatePostUrl)]
    public async Task<ActionResult<CatalogResponse>> NavigateCatalog(
        [FromBody][Required(AllowEmptyStrings = false)] CatalogRequest request,
        CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested) return StatusCode(503);

        var dto = request.MapToDto();
        var response = await catalogService.NavigateCatalog(dto, cancellationToken);
        var catalogResponse = response.MapFromDto();
        return Ok(catalogResponse);
    }
}
