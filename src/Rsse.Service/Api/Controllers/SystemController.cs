using System.Threading;
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
public class SystemController(
    IOptionsSnapshot<DatabaseOptions> options,
    ITokenizerService tokenizerService) : ControllerBase
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
    /// Дожидаться прогрева токенизатора с учётом таймаута.
    /// </summary>
    [HttpGet(RouteConstants.SystemWaitWarmUpGetUrl)]
    public async Task<ActionResult> WaitReadinessWithTimeout(int timeoutMs = 2500, CancellationToken stoppingToken = default)
    {
        var linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
        linkedTokenSource.CancelAfter(timeoutMs);
        var linkedToken = linkedTokenSource.Token;
        while (true)
        {
            if (await tokenizerService.WaitWarmUp(linkedToken)) break;
        }

        return Ok();
    }
}
