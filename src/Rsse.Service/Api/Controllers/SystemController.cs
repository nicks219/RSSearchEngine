using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SearchEngine.Data.Configuration;
using SearchEngine.Service.ApiModels;
using SearchEngine.Service.Configuration;
using SearchEngine.Service.Contracts;

namespace SearchEngine.Api.Controllers;

/// <summary>
/// Контроллер, поставляющий системную информацию.
/// </summary>
[ApiController]
public class SystemController(IOptionsSnapshot<DatabaseOptions> options, ITokenizerService tokenizer) : ControllerBase
{
    /// <summary>
    /// Получить версию сервиса.
    /// </summary>
    [HttpGet(RouteConstants.SystemVersionGetUrl)]
    public ActionResult GetVersion()
    {
        var response = new SystemResponse(
            Version: Constants.ApplicationFullName,
            DebugBuild: Constants.IsDebug,
            ReaderContext: options.Value.ReaderContext,
            CreateTablesOnPgMigration: options.Value.CreateTablesOnPgMigration
        );
        return Ok(response);
    }

    /// <summary>
    /// Дождаться прогрева токенизатора.
    /// </summary>
    [HttpGet(RouteConstants.SystemWaitWarmUpGetUrl)]
    public async Task<ActionResult> WaitReadiness()
    {
        await tokenizer.WaitWarmUp();
        return Ok();
    }
}
