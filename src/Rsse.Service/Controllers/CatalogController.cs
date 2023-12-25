using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SearchEngine.Data.Dto;
using SearchEngine.Infrastructure.Tokenizer.Contracts;
using SearchEngine.Service.Models;

namespace SearchEngine.Controllers;

[Route("api/catalog")]
[ApiController]
public class CatalogController : ControllerBase
{
    public const string NavigateCatalogError = $"[{nameof(CatalogController)}] {nameof(NavigateCatalog)} error";
    private const string ReadCatalogPageError = $"[{nameof(CatalogController)}:] {nameof(ReadCatalogPage)} error";
    private const string DeleteNoteError = $"[{nameof(CatalogController)}] {nameof(DeleteNote)} error";

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
            _logger.LogError(ex, ReadCatalogPageError);
            return new CatalogDto { ErrorMessage = ReadCatalogPageError };
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
            _logger.LogError(ex, NavigateCatalogError);
            return new CatalogDto { ErrorMessage = NavigateCatalogError };
        }
    }

    [Authorize]
    [HttpDelete]
    public async Task<ActionResult<CatalogDto>> DeleteNote(int id, int pg)
    {
        try
        {
            using var scope = _serviceScopeFactory.CreateScope();

            var tokenizer = scope.ServiceProvider.GetRequiredService<ITokenizerService>();
            tokenizer.Delete(id);

            return await new CatalogModel(scope).DeleteNote(id, pg);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, DeleteNoteError);
            return new CatalogDto { ErrorMessage = DeleteNoteError };
        }
    }
}
