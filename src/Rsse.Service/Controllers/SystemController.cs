using Microsoft.AspNetCore.Mvc;
using SearchEngine.Common.Auth;

namespace SearchEngine.Controllers;

/// <summary>
/// Контроллер, поставляющий системную информацию.
/// </summary>
[Route("system")]
public class SystemController : Controller
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
            DebugBuild = isDebug
        });
    }
}
