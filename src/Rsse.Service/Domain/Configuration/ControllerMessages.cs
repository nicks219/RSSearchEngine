using SearchEngine.Api.Controllers;

namespace SearchEngine.Domain.Configuration;

/// <summary>
/// Сообщения для функционала логирования.
/// </summary>
internal abstract class ControllerMessages
{
    internal const string LoginError = $"[{nameof(AccountController)}] {nameof(AccountController.Login)} system error";
    internal const string DataError = $"[{nameof(AccountController)}] credentials error";
    internal const string LogOutMessage = $"[{nameof(AccountController)}] {nameof(AccountController.Logout)}";
    internal const string LoginOkMessage = $"[{nameof(AccountController)}] {nameof(AccountController.Login)}";
    internal const string ModifyCookieMessage = $"[{nameof(AccountController)}] {nameof(AccountController.ModifyCookie)}";

    internal const string NavigateCatalogError = $"[{nameof(CatalogController)}] {nameof(CatalogController.NavigateCatalog)} error";
    internal const string ReadCatalogPageError = $"[{nameof(CatalogController)}] {nameof(CatalogController.ReadCatalogPage)} error";
    internal const string DeleteNoteError = $"[{nameof(CatalogController)}] {nameof(CatalogController.DeleteNote)} error";

    internal const string FindError = $"[{nameof(ComplianceController)}] {nameof(ComplianceController.GetComplianceIndices)} error: search indices may corrupted";

    internal const string CreateNoteError = $"[{nameof(CreateController)}] {nameof(CreateController.CreateNoteAndDumpAsync)} error";
    internal const string GetTagListError = $"[{nameof(CreateController)}] {nameof(CreateController.GetStructuredTagListAsync)} error";

    internal const string CreateError = $"[{nameof(MigrationController)}] {nameof(MigrationController.CreateDump)} error";
    internal const string RestoreError = $"[{nameof(MigrationController)}] {nameof(MigrationController.RestoreFromDump)} error";

    internal const string ElectNoteError = $"[{nameof(ReadController)}] {nameof(ReadController.GetNextOrSpecificNote)} error";
    internal const string ReadTitleByNoteIdError = $"[{nameof(ReadController)}] {nameof(ReadController.ReadTitleByNoteId)} error";
    internal const string ReadTagListError = $"[{nameof(ReadController)}] {nameof(ReadController.ReadTagList)} error";

    internal const string GetInitialNoteError = $"[{nameof(UpdateController)}] {nameof(UpdateController.GetInitialNote)} error";
    internal const string UpdateNoteError = $"[{nameof(UpdateController)}] {nameof(UpdateController.UpdateNote)} error";
}
