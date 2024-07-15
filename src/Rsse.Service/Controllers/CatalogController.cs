using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SearchEngine.Common.Auth;
using SearchEngine.Data.Dto;
using SearchEngine.Engine.Contracts;
using SearchEngine.Models;

namespace SearchEngine.Controllers;

/// <summary>
/// Контроллер для функционала каталога с возможностью удаления заметки
/// </summary>

[Route("api/catalog"), ApiController]

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

    /// <summary>
    /// Прочитать страницу каталога
    /// </summary>
    /// <param name="id">номер страницы</param>
    [HttpGet]
    public async Task<ActionResult<CatalogDto>> ReadCatalogPage(int id)
    {
        try
        {
            using var scope = _serviceScopeFactory.CreateScope();
            return await new CatalogModel(scope).ReadPage(id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ReadCatalogPageError);
            return new CatalogDto { ErrorMessage = ReadCatalogPageError };
        }
    }

    /// <summary>
    /// Переместиться по каталогу
    /// </summary>
    /// <param name="dto">шаблон с информацией для навигации</param>
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

    /// <summary>
    /// Удалить заметку
    /// </summary>
    /// <param name="id">идентификатор заметки</param>
    /// <param name="pg">номер страницы каталога с удаляемой заметкой</param>
    /// <returns>актуальная страница каталога</returns>
    [Authorize, HttpDelete]
    [Authorize(Constants.FullAccessPolicyName)]
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
