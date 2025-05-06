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
/// Контроллер для удаления сущностей
/// </summary>
[ApiController]
[ApiExplorerSettings(IgnoreApi = !Constants.IsDebug)]
public class DeleteController(
    IDataRepository repo,
    ITokenizerService tokenizer,
    ILogger<CatalogController> logger,
    ILogger<CatalogManager> managerLogger) : ControllerBase
{
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
