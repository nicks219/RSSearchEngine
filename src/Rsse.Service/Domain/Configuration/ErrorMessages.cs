using SearchEngine.Domain.Managers;

namespace SearchEngine.Domain.Configuration;

/// <summary>
/// Сообщения для функционала логирования.
/// </summary>
internal abstract class ErrorMessages
{
    internal const string NavigateCatalogError = $"[{nameof(CatalogManager)}] {nameof(CatalogManager.NavigateCatalog)} error";
    internal const string ReadCatalogPageError = $"[{nameof(CatalogManager)}] {nameof(CatalogManager.ReadPage)} error";

    internal const string DeleteNoteError = $"[{nameof(DeleteManager)}] {nameof(DeleteManager.DeleteNote)} error";

    internal const string CreateManagerReadTagListError = $"[{nameof(CreateManager)}] {nameof(CreateManager.ReadStructuredTagList)} error";
    internal const string CreateNoteError = $"[{nameof(CreateManager)}] {nameof(CreateManager.CreateNote)} error";
    internal const string CreateNoteUnsuccessfulError = $"[{nameof(CreateManager)}] {nameof(CreateManager.CreateNote)} error: create unsuccessful";
    internal const string CreateNoteEmptyDataError = $"[{nameof(CreateManager)}] {nameof(CreateManager.CreateNote)} error: empty data";

    internal const string SignInError = $"[{nameof(AccountManager)}] {nameof(AccountManager.TrySignInWith)} system error";

    internal const string ElectNoteError = $"[{nameof(ReadManager)}] {nameof(ReadManager.GetNextOrSpecificNote)} error";
    internal const string ReadTitleByNoteIdError = $"[{nameof(ReadManager)}] {nameof(ReadManager.ReadTitleByNoteId)} error";
    internal const string ReadModelReadTagListError = $"[{nameof(ReadManager)}] {nameof(ReadManager.ReadTagList)} error";

    internal const string GetOriginalNoteError = $"[{nameof(UpdateManager)}] {nameof(UpdateManager.GetNoteWithTagsForUpdate)} error";
    internal const string UpdateNoteError = $"[{nameof(UpdateManager)}] {nameof(UpdateManager.UpdateNote)} error";
}
