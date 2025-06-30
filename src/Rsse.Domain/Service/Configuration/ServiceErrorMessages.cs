using SearchEngine.Service.Api;
using CreateService = SearchEngine.Service.Api.CreateService;

namespace SearchEngine.Service.Configuration;

/// <summary>
/// Ошибки сервисов.
/// </summary>
internal static class ServiceErrorMessages
{
    internal const string CreateNoteUnsuccessfulError = $"[{nameof(CreateService)}] {nameof(CreateService.CreateNote)} error: create unsuccessful";
    internal const string CreateNoteEmptyDataError = $"[{nameof(CreateService)}] {nameof(CreateService.CreateNote)} error: empty data";

    internal const string InvalidCredosError = $"[{nameof(AccountService)}] {nameof(AccountService.TrySignInWith)} error: invalid credos";
    internal const string UserNotFoundError = $"[{nameof(AccountService)}] {nameof(AccountService.TrySignInWith)} error: user not found";
}
