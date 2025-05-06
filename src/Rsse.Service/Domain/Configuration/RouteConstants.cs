namespace SearchEngine.Domain.Configuration;

public abstract class RouteConstants
{
    // account controller
    // требует исправления, если используется вне контроллера не как часть сегмента
    public const string Account = "account";
    // продолжение route
    public const string AccountLoginGetUrl = "login";
    public const string AccountLogoutGetUrl = "logout";
    public const string AccountCheckGetUrl = "check";
    public const string AccountUpdateGetUrl = "update";

    // catalog controller
    // требует исправления, если используется вне контроллера не как часть сегмента
    public const string Catalog = "api/catalog";
    // продолжение route
    public const string CatalogGetUrl = "";// дать имя
    public const string CatalogPostUrl = "";// дать имя
    // есть query, нельзя через `/`, см HttpClientExtensions
    public const string CatalogDeleteUrl = "";// дать имя

    // compliance controller
    // если используется вне контроллера не как часть сегмента, значит требует исправления
    public const string Compliance = "api/compliance";
    // продолжение route
    public const string ComplianceIndicesGetUrl = "indices";

    // create controller
    // требует исправления, если используется вне контроллера не как часть сегмента
    public const string Create = "api/create";
    // продолжение route
    // нет query, можно через `/`, см HttpClientExtensions
    public const string CreateGetUrl = "";// дать имя
    public const string CreatePostUrl = "";// дать имя

    // migration controller
    // требует исправления, если используется вне контроллера не как часть сегмента
    public const string Migration =  "migration";
    // продолжение route
    public const string MigrationCopyGetUrl =  "copy";
    public const string MigrationCreateGetUrl =  "create";
    public const string MigrationRestoreGetUrl =  "restore";
    public const string MigrationUploadPostUrl =  "upload";
    public const string MigrationDownloadGetUrl =  "download";

    // read controller
    // требует исправления, если используется вне контроллера не как часть сегмента
    public const string Read = "api/read";
    // продолжение route
    public const string ReadElectionGetUrl = "election";
    public const string ReadTitlePostUrl = "title";
    public const string ReadGetUrl = "";// дать имя
    public const string ReadPostUrl = "";// дать имя

    // system controller
    // требует исправления, если используется вне контроллера не как часть сегмента
    public const string System = "system";
    // продолжение route
    public const string SystemVersionGetUrl = "version";

    // update controller
    // требует исправления, если используется вне контроллера не как часть сегмента
    public const string Update = "api/update";
    // продолжение route
    public const string UpdateGetUrl = "";// дать имя
    public const string UpdatePostUrl = "";// дать имя
}
