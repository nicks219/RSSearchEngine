namespace SearchEngine.Service.Configuration;

/// <summary>
/// Системные константы приложения.
/// </summary>
public abstract class Constants
{
    /// <summary/> Выставляется для отладочной сборки.
#if DEBUG
    internal const bool IsDebug = true;
#else
    internal const bool IsDebug = false;
#endif

    /// <summary/> Мажорная версия.
    internal const string MajorVersion = "6";
    /// <summary/> Минорная версия.
    private const string MinorVersion = "0";
    /// <summary/> Патч.
    private const string PatchVersion = "0";

    // <summary/> Рут для статики.
    internal const string StaticDirectory = "ClientApp/build";
    // <summary/> Расширение для дампа mysql.
    internal const string MySqlDumpExt = ".dump";
    // <summary/> Расширение для архива дампа postgres.
    internal const string PostgresDumpArchiveName = "dump.zip";

    // <summary>Версия приложения.</summary>
    internal const string ApplicationVersion = $"{MajorVersion}.{MinorVersion}.{PatchVersion}";
    // <summary>Версия API.</summary>
    internal const string ApiVersion = $"{MajorVersion}.{MinorVersion}";
    // <summary>Полное название версии приложения.</summary>
    internal const string ApplicationFullName = $"v{ApplicationVersion}: pre-release 3 | .NET9/React19/PostgreSQL + MySql " +
                                                $"| code-review | Open Telemetry";
    // <summary>Именование документации OpenAPI, транслируется в сегмент пути к описанию.</summary>
    internal const string SwaggerDocNameSegment = $"v{MajorVersion}";
    // <summary>Именование заголовка Swagger.</summary>
    internal const string SwaggerTitle = "RSSearchEngine API";
    // <summary>Именование политики полного доступа.</summary>
    internal const string FullAccessPolicyName = nameof(FullAccessPolicyName);
    // <summary>Именование политики CORS для разработки.</summary>
    internal const string DevelopmentCorsPolicy = nameof(DevelopmentCorsPolicy);
    // <summary>Именование политики RL для технической ручки.</summary>
    internal const string MetricsHandlerPolicy = nameof(MetricsHandlerPolicy);
    // <summary>Утверждение для проверки внутреннего идентификатора.</summary>
    internal const string IdInternalClaimType = nameof(IdInternalClaimType);
    // <summary>Идентификатор администратора.</summary>
    internal const string AdminId = "1";

    // <summary>Именование окружения для тестирования.</summary>
    internal const string TestingEnvironment = "Testing";
    // <summary>Именование переменной, задающей имя окружения.</summary>
    internal const string AspNetCoreEnvironmentName = "ASPNETCORE_ENVIRONMENT";

    // <summary>Именование переменной, задающей имя окружения.</summary>
    internal const string AspNetCoreObservabilityDisableName = "ASPNETCORE_OBSERVABILITY_DISABLE";
    // <summary>Именование окружения для тестирования.</summary>
    internal const string DisableValue = "Disable";

    // <summary>Заголовок для челенджа аутентификации.</summary>
    internal const string ShiftHeaderName = "Shift";
    // <summary>Значение заголовка для челенджа.</summary>
    internal const string ShiftHeaderValue = "301 Cancelled";
}
