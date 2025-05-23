using SearchEngine.Api.Controllers;

namespace SearchEngine.Api.Messages;

/// <summary>
/// Ошибки контроллеров.
/// </summary>
public abstract class ControllerErrorMessages
{
    internal const string LoginError = $"[{nameof(AccountController)}] {nameof(AccountController.Login)} system error";
    internal const string LogoutError = $"[{nameof(AccountController)}] {nameof(AccountController.Logout)} system error";
    internal const string UpdateCredosError = $"[{nameof(AccountController)}] {nameof(AccountController.UpdateCredos)} system error";
    internal const string DataError = $"[{nameof(AccountController)}] {nameof(AccountController.Login)} credentials error";
    internal const string RedirectError = $"[{nameof(AccountController)}] {nameof(AccountController.Login)} redirect not supported error";

    internal const string NavigateCatalogError = $"[{nameof(CatalogController)}] {nameof(CatalogController.NavigateCatalog)} error";
    internal const string ReadCatalogPageError = $"[{nameof(CatalogController)}] {nameof(CatalogController.ReadCatalogPage)} error";
    internal const string DeleteNoteError = $"[{nameof(DeleteController)}] {nameof(DeleteController.DeleteNote)} error";

    internal const string ComplianceError = $"[{nameof(ComplianceSearchController)}] {nameof(ComplianceSearchController.GetComplianceIndices)} error: search indices may corrupted";

    internal const string CreateNoteError = $"[{nameof(CreateController)}] {nameof(CreateController.CreateNoteAndDumpAsync)} error";

    internal const string CreateError = $"[{nameof(MigrationController)}] {nameof(MigrationController.CreateDump)} error";
    internal const string RestoreError = $"[{nameof(MigrationController)}] {nameof(MigrationController.RestoreFromDump)} error";
    internal const string UploadError = $"[{nameof(MigrationController)}] {nameof(MigrationController.UploadFile)} error";
    internal const string DownloadError = $"[{nameof(MigrationController)}] {nameof(MigrationController.DownloadFile)} error";
    internal const string CopyError = $"[{nameof(MigrationController)}] {nameof(MigrationController.CopyFromMySqlToPostgres)} error";

    internal const string ElectNoteError = $"[{nameof(ReadController)}] {nameof(ReadController.GetNextOrSpecificNote)} error";
    internal const string ReadTitleByNoteIdError = $"[{nameof(ReadController)}] {nameof(ReadController.ReadTitleByNoteId)} error";
    internal const string ReadTagListError = $"[{nameof(ReadController)}] {nameof(ReadController.ReadTagList)} error";
    internal const string GetTagListForCreateError = $"[{nameof(ReadController)}] {nameof(ReadController.GetStructuredTagListForCreate)} error";
    internal const string GetNoteWithTagsForUpdateError = $"[{nameof(ReadController)}] {nameof(ReadController.GetNoteWithTagsForUpdate)} error";

    internal const string UpdateNoteError = $"[{nameof(UpdateController)}] {nameof(UpdateController.UpdateNote)} error";
}
