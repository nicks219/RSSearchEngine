using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SearchEngine.Common.Auth;
using SearchEngine.Data.Repository;

namespace SearchEngine.Controllers;

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
        var isDebug = false;
#if DEBUG
        isDebug = true;
#endif
        return Ok(new
        {
            Version = Constants.ApplicationFullName,
            DebugBuild = isDebug,
            ReaderContext = options.Value.ReaderContext,
            CreateTablesOnPgMigration = options.Value.CreateTablesOnPgMigration
        });
    }
}
