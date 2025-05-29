using SearchEngine.Api.Controllers;

namespace SearchEngine.Api.Configuration;

/// <summary>
/// Ошибки контроллеров.
/// </summary>
public abstract class ControllerErrorMessages
{
    internal const string RedirectError = $"[{nameof(AccountController)}] {nameof(AccountController.Login)} redirect not supported error";
}
