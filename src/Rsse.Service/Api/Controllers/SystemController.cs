using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SearchEngine.Domain.Configuration;
using SearchEngine.Domain.Contracts;

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
        return Ok(new
        {
            Version = Constants.ApplicationFullName,
            DebugBuild = Constants.IsDebug,
            ReaderContext = options.Value.ReaderContext,
            CreateTablesOnPgMigration = options.Value.CreateTablesOnPgMigration
        });
    }

    /// <summary>
    /// Дождаться прогрева токенизатора
    /// </summary>
    [HttpGet(RouteConstants.SystemWaitWarmUpGetUrl)]
    public async Task<ActionResult> WaitReadiness()
    {
        await tokenizer.WaitWarmUp();
        return Ok();
    }
}
