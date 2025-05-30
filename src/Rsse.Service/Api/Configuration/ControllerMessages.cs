using SearchEngine.Api.Controllers;

namespace SearchEngine.Api.Configuration;

/// <summary>
/// Сообщения контроллеров.
/// </summary>
internal abstract class ControllerMessages : ControllerErrorMessages
{
    internal const string LogOutMessage = $"[{nameof(AccountController)}] {nameof(AccountController.Logout)}";
    internal const string LoginOkMessage = $"[{nameof(AccountController)}] {nameof(AccountController.Login)}";
}
