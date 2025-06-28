namespace Rsse.Domain.Service.Configuration;

public abstract class RouteConstants
{
    private const string Version = $"v{Constants.MajorVersion}";

    // account controller
    public const string AccountLoginGetUrl = $"/{Version}/account/login";
    public const string AccountLogoutGetUrl = $"/{Version}/account/logout";
    public const string AccountCheckGetUrl = $"/{Version}/account/check";
    // [глагол] можно поправить на PUT или POST, перенести в update
    public const string AccountUpdateGetUrl = $"/{Version}/account/update";

    // catalog controller
    public const string CatalogPageGetUrl = $"/{Version}/catalog";
    // [глагол] можно поправить на GET - в запросе используется только номер станицы и "направление" навигации
    public const string CatalogNavigatePostUrl = $"/{Version}/catalog/navigate";

    // compliance controller
    public const string ComplianceIndicesGetUrl = $"/{Version}/compliance/indices";

    // create controller | recovery не учитывает HTTP глаголы
    public const string CreateNotePostUrl = $"/{Version}/note/create";

    // delete controller | recovery не учитывает HTTP глаголы
    public const string DeleteNoteUrl = $"/{Version}/note/delete";

    // read controller
    public const string ReadElectionGetUrl = $"/{Version}/election/switch";
    public const string ReadTitleGetUrl = $"/{Version}/title";
    // [глагол] /v1/note?id=... можно поправить на GET с BODY (нарушение RFC 7231) или оставить POST
    public const string ReadNotePostUrl = $"/{Version}/election/note";
    // дублирует [CreateGetTagsUrl] | без авторизации
    public const string ReadTagsGetUrl = $"/{Version}/tags";
    // дублирует [ReadGetTagsUrl] | под авторизацией | recovery не учитывает HTTP глаголы
    public const string ReadTagsForCreateAuthGetUrl = $"/{Version}/tags/forCreate";
    // под авторизацией | recovery не учитывает HTTP глаголы
    public const string ReadNoteWithTagsForUpdateAuthGetUrl = $"/{Version}/note/forUpdate";

    // migration controller
    public const string MigrationCopyGetUrl = "/migration/copy";
    public const string MigrationCreateGetUrl = "/migration/create";
    public const string MigrationRestoreGetUrl = "/migration/restore";
    public const string MigrationUploadPostUrl = "/migration/upload";
    public const string MigrationDownloadGetUrl = "/migration/download";

    // system controller [k3s пробы]
    public const string SystemVersionGetUrl = "/system/version";
    public const string SystemWaitWarmUpGetUrl = "/system/warmup/wait";

    // update controller | recovery не учитывает HTTP глаголы
    public const string UpdateNotePutUrl = $"/{Version}/note/update";
}
