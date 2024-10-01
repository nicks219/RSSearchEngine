using SearchEngine.Models;

namespace SearchEngine.Common;

/// <summary>
/// Сообщения для функционала логирования.
/// </summary>
internal abstract class ModelMessages
{
    internal const string NavigateCatalogError = $"[{nameof(CatalogModel)}] {nameof(CatalogModel.NavigateCatalog)} error";
    internal const string ReadCatalogPageError = $"[{nameof(CatalogModel)}] {nameof(CatalogModel.ReadPage)} error";
    internal const string DeleteNoteError = $"[{nameof(CatalogModel)}] {nameof(CatalogModel.DeleteNote)} error";

    internal const string CreateModelReadTagListError = $"[{nameof(CreateModel)}] {nameof(CreateModel.ReadStructuredTagList)} error";
    internal const string CreateNoteError = $"[{nameof(CreateModel)}] {nameof(CreateModel.CreateNote)} error";
    internal const string CreateNoteUnsuccessfulError = $"[{nameof(CreateModel)}] {nameof(CreateModel.CreateNote)} error: create unsuccessful";
    internal const string CreateNoteEmptyDataError = $"[{nameof(CreateModel)}] {nameof(CreateModel.CreateNote)} error: empty data";

    internal const string SignInError = $"[{nameof(LoginModel)}] {nameof(LoginModel.SignIn)} system error";

    internal const string ElectNoteError = $"[{nameof(ReadModel)}] {nameof(ReadModel.GetNextOrSpecificNote)} error";
    internal const string ReadTitleByNoteIdError = $"[{nameof(ReadModel)}] {nameof(ReadModel.ReadTitleByNoteId)} error";
    internal const string ReadModelReadTagListError = $"[{nameof(ReadModel)}] {nameof(ReadModel.ReadTagList)} error";

    internal const string GetOriginalNoteError = $"[{nameof(UpdateModel)}] {nameof(UpdateModel.GetOriginalNote)} error";
    internal const string UpdateNoteError = $"[{nameof(UpdateModel)}] {nameof(UpdateModel.UpdateNote)} error";
}
