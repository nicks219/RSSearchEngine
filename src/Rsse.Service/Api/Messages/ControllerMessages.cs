using SearchEngine.Api.Controllers;

namespace SearchEngine.Api.Messages;

/// <summary>
/// Сообщения контроллеров, для функционала логирования.
/// </summary>
internal abstract class ControllerMessages
{
    internal const string OkMessage = "[Ok]";
    internal const string LogOutMessage = $"[{nameof(AccountController)}] {nameof(AccountController.Logout)}";
    internal const string LoginOkMessage = $"[{nameof(AccountController)}] {nameof(AccountController.Login)}";
    internal const string ModifyCookieMessage = $"[{nameof(AccountController)}] {nameof(AccountController.ModifyCookie)}";
}
