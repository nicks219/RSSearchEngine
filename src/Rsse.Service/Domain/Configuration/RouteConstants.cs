namespace SearchEngine.Domain.Configuration;

public abstract class RouteConstants
{
    private const string Version = $"v{Constants.MajorVersion}";

    // compliance controller
    public const string ComplianceIndicesGetUrl = $"/{Version}/compliance/indices";

    // catalog controller
    public const string CatalogPageGetUrl = $"/{Version}/catalog";
    // [глагол] поправить на GET
    public const string CatalogNavigatePostUrl = $"/{Version}/catalog/navigate";

    // delete controller | recovery не учитывает HTTP глаголы
    public const string CatalogDeleteNoteUrl = $"/{Version}/note/delete";

    // create controller
    // /v1/note ... POST | recovery не учитывает HTTP глаголы
    public const string CreateNotePostUrl = $"/{Version}/note/create";

    // update controller
    // [глагол] /v1/note поправить на PATCH или PUT | recovery не учитывает HTTP глаголы
    public const string UpdateNotePostUrl = $"/{Version}/note/update";

    // read controller
    public const string ReadElectionGetUrl = $"/{Version}/election/switch";
    public const string ReadTitleGetUrl = $"/{Version}/title";
    // дублирует [CreateGetTagsUrl] | без авторизации
    public const string ReadGetTagsUrl = $"/{Version}/tags";
    // [глагол] /v1/note?id=... поправить на GET
    public const string ReadNotePostUrl = $"/{Version}/election/note";
    // дублирует [ReadGetTagsUrl] | под авторизацией | recovery не учитывает HTTP глаголы
    public const string CreateGetTagsAuthorizedUrl = $"/{Version}/tags/forCreate";
    // под авторизацией | recovery не учитывает HTTP глаголы
    public const string UpdateGetNoteWithTagsUrl = $"/{Version}/note/forUpdate";

    // account controller
    public const string AccountLoginGetUrl = $"/{Version}/account/login";
    public const string AccountLogoutGetUrl = $"/{Version}/account/logout";
    public const string AccountCheckGetUrl = $"/{Version}/account/check";
    // [глагол] поправить на POST и перенести в update
    public const string AccountUpdateGetUrl = $"/{Version}/account/update";

    // migration controller
    public const string MigrationCopyGetUrl = "/migration/copy";
    public const string MigrationCreateGetUrl = "/migration/create";
    public const string MigrationRestoreGetUrl = "/migration/restore";
    public const string MigrationUploadPostUrl = "/migration/upload";
    public const string MigrationDownloadGetUrl = "/migration/download";

    // system controller [k3s пробы]
    public const string SystemVersionGetUrl = "/system/version";
}
