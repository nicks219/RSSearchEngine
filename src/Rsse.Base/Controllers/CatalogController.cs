using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RandomSongSearchEngine.Data.Dto;
using RandomSongSearchEngine.Infrastructure.Cache.Contracts;
using RandomSongSearchEngine.Service.Models;

namespace RandomSongSearchEngine.Controllers;

[Route("api/catalog")]
[ApiController]
public class CatalogController : ControllerBase
{
    private readonly ILogger<CatalogController> _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public CatalogController(IServiceScopeFactory serviceScopeFactory, ILogger<CatalogController> logger)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<CatalogDto>> ReadCatalogPage(int id)
    {
        try
        {
            using var scope = _serviceScopeFactory.CreateScope();
            return await new CatalogModel(scope).ReadCatalogPage(id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"[{nameof(CatalogController)}: {nameof(ReadCatalogPage)} error]");
            return new CatalogDto { ErrorMessage = $"[{nameof(CatalogController)}: {nameof(ReadCatalogPage)} error]" };
        }
    }

    [HttpPost]
    public async Task<ActionResult<CatalogDto>> NavigateCatalog([FromBody] CatalogDto dto)
    {
        try
        {
            using var scope = _serviceScopeFactory.CreateScope();
            return await new CatalogModel(scope).NavigateCatalog(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"[{nameof(CatalogController)}: {nameof(NavigateCatalog)} error]");
            return new CatalogDto { ErrorMessage = $"[{nameof(CatalogController)}: {nameof(NavigateCatalog)} error]" };
        }
    }

    [Authorize]
    [HttpDelete]
    public async Task<ActionResult<CatalogDto>> DeleteNote(int id, int pg)
    {
        try
        {
            using var scope = _serviceScopeFactory.CreateScope();
            
            var cache = scope.ServiceProvider.GetRequiredService<ICacheRepository>();
            cache.Delete(id);
            
            return await new CatalogModel(scope).DeleteNote(id, pg);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"[{nameof(CatalogController)}: {nameof(DeleteNote)} error]");
            return new CatalogDto { ErrorMessage = $"[{nameof(CatalogController)}: {nameof(DeleteNote)} error]" };
        }
    }
}
