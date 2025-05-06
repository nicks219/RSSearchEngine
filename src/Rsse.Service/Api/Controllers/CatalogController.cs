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
/// Контроллер для функционала каталога с возможностью удаления заметки
/// </summary>
[ApiController]
[ApiExplorerSettings(IgnoreApi = !Constants.IsDebug)]
public class CatalogController(
    IDataRepository repo,
    ITokenizerService tokenizer,
    ILogger<CatalogController> logger,
    ILogger<CatalogManager> managerLogger) : ControllerBase
{
    /// <summary>
    /// Прочитать страницу каталога
    /// </summary>
    /// <param name="id">номер страницы</param>
    [HttpGet(RouteConstants.CatalogPageGetUrl)]
    public async Task<ActionResult<CatalogResponse>> ReadCatalogPage(int id)
    {
        try
        {
            var response = await new CatalogManager(repo, managerLogger).ReadPage(id);
            return response.MapFromDto();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ReadCatalogPageError);
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
            var response = await new CatalogManager(repo, managerLogger).NavigateCatalog(dto);
            return response.MapFromDto();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, NavigateCatalogError);
            return new CatalogResponse { ErrorMessage = NavigateCatalogError };
        }
    }

    /// <summary>
    /// Удалить заметку
    /// </summary>
    /// <param name="id">идентификатор заметки</param>
    /// <param name="pg">номер страницы каталога с удаляемой заметкой</param>
    /// <returns>актуальная страница каталога</returns>
    [Authorize, HttpDelete(RouteConstants.CatalogDeleteNoteUrl)]
    [Authorize(Constants.FullAccessPolicyName)]
    public async Task<ActionResult<CatalogResponse>> DeleteNote(int id, int pg)
    {
        try
        {
            tokenizer.Delete(id);

            var response = await new CatalogManager(repo, managerLogger).DeleteNote(id, pg);
            return response.MapFromDto();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, DeleteNoteError);
            return new CatalogResponse { ErrorMessage = DeleteNoteError };
        }
    }
}
