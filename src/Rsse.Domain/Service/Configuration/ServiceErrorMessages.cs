using SearchEngine.Services;

namespace SearchEngine.Service.Configuration;

/// <summary>
/// Ошибки сервисов.
/// </summary>
internal static class ServiceErrorMessages
{
    internal const string CreateNoteUnsuccessfulError = $"[{nameof(CreateService)}] {nameof(CreateService.CreateNote)} error: create unsuccessful";
    internal const string CreateNoteEmptyDataError = $"[{nameof(CreateService)}] {nameof(CreateService.CreateNote)} error: empty data";
}
