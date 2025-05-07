using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SearchEngine.Api.Mapping;
using SearchEngine.Domain.ApiModels;
using SearchEngine.Domain.Configuration;
using SearchEngine.Domain.Contracts;
using SearchEngine.Domain.Managers;
using static SearchEngine.Domain.Configuration.ControllerMessages;

namespace SearchEngine.Api.Controllers;

/// <summary>
/// Контроллер для функционала каталога
/// </summary>
[ApiController]
[ApiExplorerSettings(IgnoreApi = !Constants.IsDebug)]
public class CatalogController(
    IDataRepository repo,
    ILoggerFactory loggerFactory) : ControllerBase
{
    private readonly ILogger<CatalogController> _logger = loggerFactory.CreateLogger<CatalogController>();
    private readonly ILogger<CatalogManager> _managerLogger = loggerFactory.CreateLogger<CatalogManager>();

    /// <summary>
    /// Прочитать страницу каталога
    /// </summary>
    /// <param name="id">номер страницы</param>
    [HttpGet(RouteConstants.CatalogPageGetUrl)]
    public async Task<ActionResult<CatalogResponse>> ReadCatalogPage(int id)
    {
        try
        {
            var response = await new CatalogManager(repo, _managerLogger).ReadPage(id);
            return response.MapFromDto();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ReadCatalogPageError);
            return new CatalogResponse { ErrorMessage = ReadCatalogPageError };
        }
    }

    /// <summary>
    /// Переместиться по каталогу
    /// </summary>
    /// <param name="request">шаблон с информацией для навигации</param>
    [HttpPost(RouteConstants.CatalogNavigatePostUrl)]
    public async Task<ActionResult<CatalogResponse>> NavigateCatalog([FromBody] CatalogRequest request)
    {
        try
        {
            var dto = request.MapToDto();
            var response = await new CatalogManager(repo, _managerLogger).NavigateCatalog(dto);
            return response.MapFromDto();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, NavigateCatalogError);
            return new CatalogResponse { ErrorMessage = NavigateCatalogError };
        }
    }
}
