using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Rsse.Domain.Data.Configuration;
using Rsse.Domain.Service.ApiModels;
using Rsse.Domain.Service.Configuration;
using Rsse.Domain.Service.Contracts;

namespace Rsse.Api.Controllers;

/// <summary>
/// Контроллер, поставляющий системную информацию.
/// </summary>
[ApiController]
public class SystemController(
    IOptionsSnapshot<DatabaseOptions> databaseOptions,
    IOptionsMonitor<ElectionTypeOptions> electionType,
    ITokenizerApiClient tokenizerApiClient) : ControllerBase
{
    /// <summary>
    /// Дефолтный таймаут ожидания прогрева сервиса, используется в <see cref="WaitReadinessWithTimeout"/>.
    /// </summary>
    private const int ReadinessTimeoutDefaultMs = 5000;

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
            ReaderContext: databaseOptions.Value.ReaderContext,
            CreateTablesOnPgMigration: databaseOptions.Value.CreateTablesOnPgMigration,
            ElectionType: electionType.CurrentValue.ElectionType.ToString()
        );
        return Ok(response);
    }

    /// <summary>
    /// Ждать прогрев токенизатора, с учётом таймаута.
    /// </summary>
    /// <param name="timeoutMs">Максимальное время ожидания прогрева.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns><b>200</b> - прогрев завершен, <b>503</b> - прогрев не завершился до таймаута.</returns>
    [HttpGet(RouteConstants.SystemWaitWarmUpGetUrl)]
    public async Task<ActionResult> WaitReadinessWithTimeout(int timeoutMs = ReadinessTimeoutDefaultMs,
        CancellationToken cancellationToken = default)
    {
        var linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        linkedTokenSource.CancelAfter(timeoutMs);
        var timeoutToken = linkedTokenSource.Token;

        while (true)
        {
            if (timeoutToken.IsCancellationRequested) return StatusCode(503, nameof(WaitReadinessWithTimeout));
            if (await tokenizerApiClient.WaitWarmUp(timeoutToken)) break;
        }

        return Ok();
    }
}
