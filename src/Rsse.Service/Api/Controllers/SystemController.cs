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
    public ActionResult GetVersion(CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested) return StatusCode(503);

        var response = new SystemResponse(
            Version: Constants.ApplicationFullName,
            DebugBuild: Constants.IsDebug,
            ReaderContext: options.Value.ReaderContext,
            CreateTablesOnPgMigration: options.Value.CreateTablesOnPgMigration
        );
        return Ok(response);
    }

    /// <summary>
    /// Ждать прогрев токенизатора, с учётом таймаута.
    /// </summary>
    /// <param name="timeoutMs">Таймаут.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>200 - прогрев завершен, 503 - прогрев не завершился до таймаута.</returns>
    [HttpGet(RouteConstants.SystemWaitWarmUpGetUrl)]
    public async Task<ActionResult> WaitReadinessWithTimeout(int timeoutMs = 5000,
        CancellationToken cancellationToken = default)
    {
        var linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        linkedTokenSource.CancelAfter(timeoutMs);
        var timeoutToken = linkedTokenSource.Token;
        if (timeoutToken.IsCancellationRequested) return StatusCode(503, nameof(WaitReadinessWithTimeout));

        while (true)
        {
            if (await tokenizerService.WaitWarmUp(timeoutToken)) break;
        }

        return Ok();
    }
}
