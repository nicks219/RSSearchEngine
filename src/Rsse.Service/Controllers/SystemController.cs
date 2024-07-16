using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SearchEngine.Common.Auth;

namespace SearchEngine.Controllers;

/// <summary>
/// Контроллер с системным функионалом для проверок
/// </summary>
[Route("system")]
public class SystemController : Controller
{
    /// <summary>
    /// Получить версию сервиса
    /// </summary>
    /// <returns></returns>
    [HttpGet("version")]
    public ActionResult GetVersion()
    {
        return Ok(Constants.ApplicationFullName);
    }
}
