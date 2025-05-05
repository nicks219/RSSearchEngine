using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SearchEngine.Domain.Configuration;

namespace SearchEngine.Api.Controllers;

/// <summary>
/// Контроллер, поставляющий системную информацию.
/// </summary>
[Route("system")]
public class SystemController(IOptionsSnapshot<DatabaseOptions> options) : Controller
{
    /// <summary>
    /// Получить версию сервиса.
    /// </summary>
    /// <returns></returns>
    [HttpGet("version")]
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
}
