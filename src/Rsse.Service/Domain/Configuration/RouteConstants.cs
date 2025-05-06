namespace SearchEngine.Domain.Configuration;

public abstract class RouteConstants
{
    // account controller
    public const string AccountLoginGetUrl = "/account/login";
    public const string AccountLogoutGetUrl = "/account/logout";
    public const string AccountCheckGetUrl = "/account/check";
    public const string AccountUpdateGetUrl = "/account/update";// антипаттерн

    // catalog controller
    public const string CatalogPageGetUrl = "/api/catalog";// различается http глаголом
    public const string CatalogNavigatePostUrl = "/api/catalog";// различается http глаголом
    public const string CatalogDeleteNoteUrl = "/api/catalog";// различается http глаголом, есть query-параметр, см HttpClientExtensions

    // compliance controller
    public const string ComplianceIndicesGetUrl = "/api/compliance/indices";

    // create controller
    // дублирует [ReadGetTagsUrl] под авторизацией
    public const string CreateGetTagsAuthorizedUrl = "/api/create";// различается http глаголом, нет query-параметров, см HttpClientExtensions
    public const string CreateNotePostUrl = "/api/create";// различается http глаголом

    // migration controller
    public const string MigrationCopyGetUrl =  "/migration/copy";
    public const string MigrationCreateGetUrl =  "/migration/create";
    public const string MigrationRestoreGetUrl =  "/migration/restore";
    public const string MigrationUploadPostUrl =  "/migration/upload";
    public const string MigrationDownloadGetUrl =  "/migration/download";

    // read controller
    public const string ReadElectionGetUrl = "/api/read/election";
    public const string ReadTitleGetUrl = "/api/read/title";
    // дублирует [CreateGetTagsUrl] без авторизации
    public const string ReadGetTagsUrl = "/api/read";// различается http глаголом
    public const string ReadNotePostUrl = "/api/read";// различается http глаголом

    // update controller
    // перенести в read
    public const string UpdateGetNoteWithTagsUrl = "/api/update";// различается http глаголом
    public const string UpdateNotePostUrl = "/api/update";// различается http глаголом

    // system controller
    public const string SystemVersionGetUrl = "/system/version";
}
