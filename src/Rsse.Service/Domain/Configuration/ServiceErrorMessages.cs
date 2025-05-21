using SearchEngine.Domain.Services;

namespace SearchEngine.Domain.Configuration;

/// <summary>
/// Сообщения сервисов, для функционала логирования.
/// </summary>
internal abstract class ServiceErrorMessages
{
    // ошибки сервисов:
    internal const string NavigateCatalogError = $"[{nameof(CatalogService)}] {nameof(CatalogService.NavigateCatalog)} error";
    internal const string ReadCatalogPageError = $"[{nameof(CatalogService)}] {nameof(CatalogService.ReadPage)} error";

    internal const string DeleteNoteError = $"[{nameof(DeleteService)}] {nameof(DeleteService.DeleteNote)} error";

    internal const string CreateManagerReadTagListError = $"[{nameof(CreateService)}] {nameof(CreateService.ReadStructuredTagList)} error";
    internal const string CreateNoteError = $"[{nameof(CreateService)}] {nameof(CreateService.CreateNote)} error";
    internal const string CreateNoteUnsuccessfulError = $"[{nameof(CreateService)}] {nameof(CreateService.CreateNote)} error: create unsuccessful";
    internal const string CreateNoteEmptyDataError = $"[{nameof(CreateService)}] {nameof(CreateService.CreateNote)} error: empty data";

    internal const string SignInError = $"[{nameof(AccountService)}] {nameof(AccountService.TrySignInWith)} system error";

    internal const string ElectNoteError = $"[{nameof(ReadService)}] {nameof(ReadService.GetNextOrSpecificNote)} error";
    internal const string ReadTitleByNoteIdError = $"[{nameof(ReadService)}] {nameof(ReadService.ReadTitleByNoteId)} error";
    internal const string ReadModelReadTagListError = $"[{nameof(ReadService)}] {nameof(ReadService.ReadTagList)} error";

    internal const string GetOriginalNoteError = $"[{nameof(UpdateService)}] {nameof(UpdateService.GetNoteWithTagsForUpdate)} error";
    internal const string UpdateNoteError = $"[{nameof(UpdateService)}] {nameof(UpdateService.UpdateNote)} error";
}
