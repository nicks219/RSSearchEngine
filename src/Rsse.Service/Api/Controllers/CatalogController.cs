using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SearchEngine.Api.Mapping;
using SearchEngine.Service.ApiModels;
using SearchEngine.Service.Configuration;
using SearchEngine.Services;
using static SearchEngine.Api.Messages.ControllerErrorMessages;

namespace SearchEngine.Api.Controllers;

/// <summary>
/// Контроллер для функционала каталога.
/// </summary>
[ApiController]
[ApiExplorerSettings(IgnoreApi = !Constants.IsDebug)]
public class CatalogController(CatalogService catalogService, ILogger<CatalogController> logger) : ControllerBase
{
    /// <summary>
    /// Прочитать страницу каталога.
    /// </summary>
    /// <param name="id">Номер страницы.</param>
    /// <param name="ct">Токен отмены.</param>
    [HttpGet(RouteConstants.CatalogPageGetUrl)]
    public async Task<ActionResult<CatalogResponse>> ReadCatalogPage(int id, CancellationToken ct)
    {
        try
        {
            var response = await catalogService.ReadPage(id, ct);
            return response.MapFromDto();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ReadCatalogPageError);
            return new CatalogResponse { ErrorMessage = ReadCatalogPageError };
        }
    }

    /// <summary>
    /// Переместиться по каталогу.
    /// </summary>
    /// <param name="request">Контейнер с информацией для навигации по каталогу.</param>
    /// <param name="ct">Токен отмены.</param>
    [HttpPost(RouteConstants.CatalogNavigatePostUrl)]
    public async Task<ActionResult<CatalogResponse>> NavigateCatalog(
        [FromBody] CatalogRequest request,
        CancellationToken ct)
    {
        try
        {
            var dto = request.MapToDto();
            var response = await catalogService.NavigateCatalog(dto, ct);
            return response.MapFromDto();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, NavigateCatalogError);
            return new CatalogResponse(ErrorMessage: NavigateCatalogError);
        }
    }
}
