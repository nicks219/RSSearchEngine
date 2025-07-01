namespace Rsse.Domain.Service.Configuration;

/// <summary>
/// Системные константы приложения.
/// </summary>
public abstract class Constants
{
    /// <summary/> Выставляется для отладочной сборки.
#if DEBUG
    public const bool IsDebug = true;
#else
    public const bool IsDebug = false;
#endif

    /// <summary/> Мажорная версия.
    internal const string MajorVersion = "6";
    /// <summary/> Минорная версия.
    private const string MinorVersion = "0";
    /// <summary/> Патч.
    private const string PatchVersion = "0";

    // <summary/> Рут для статики.
    public const string StaticDirectory = "ClientApp/build";
    // <summary/> Расширение для дампа mysql.
    public const string MySqlDumpExt = ".dump";
    // <summary/> Расширение для архива дампа postgres.
    public const string PostgresDumpArchiveName = "dump.zip";

    // <summary>Метаданные: именование сервиса.</summary>
    public const string ServiceName = "rsse-app";
    // <summary>Метаданные: именование неймспейса сервиса.</summary>
    public const string ServiceNamespace = "rsse-group";

    // <summary>Версия приложения.</summary>
    public const string ApplicationVersion = $"{MajorVersion}.{MinorVersion}.{PatchVersion}";
    // <summary>Версия API.</summary>
    public const string ApiVersion = $"{MajorVersion}.{MinorVersion}";
    // <summary>Полное название версии приложения.</summary>
    public const string ApplicationFullName = $"v{ApplicationVersion}: release | .NET9/React19/PostgreSQL + MySql " +
                                              $"| code-review | Open Telemetry | k6.0.2";
    // <summary>Именование документации OpenAPI, транслируется в сегмент пути к описанию.</summary>
    public const string SwaggerDocNameSegment = $"v{MajorVersion}";
    // <summary>Именование заголовка Swagger.</summary>
    public const string SwaggerTitle = "RSSearchEngine API";
    // <summary>Именование политики полного доступа.</summary>
    public const string FullAccessPolicyName = nameof(FullAccessPolicyName);
    // <summary>Именование политики CORS для разработки.</summary>
    public const string DevelopmentCorsPolicy = nameof(DevelopmentCorsPolicy);
    // <summary>Именование политики RL для технической ручки.</summary>
    public const string MetricsHandlerPolicy = nameof(MetricsHandlerPolicy);
    // <summary>Утверждение для проверки внутреннего идентификатора.</summary>
    public const string IdInternalClaimType = nameof(IdInternalClaimType);
    // <summary>Идентификатор администратора.</summary>
    public const string AdminId = "1";

    // <summary>Именование окружения для тестирования.</summary>
    public const string TestingEnvironment = "Testing";
    // <summary>Именование переменной, задающей имя окружения.</summary>
    public const string AspNetCoreEnvironmentName = "ASPNETCORE_ENVIRONMENT";

    // <summary>Именование переменной, задающей имя окружения.</summary>
    public const string AspNetCoreOtlpExportersDisable = "ASPNETCORE_OTLP_EXPORTERS_DISABLE";
    // <summary>Именование окружения для тестирования.</summary>
    public const string DisableValue = "Disable";

    // <summary>Заголовок для челенджа аутентификации.</summary>
    public const string ShiftHeaderName = "Shift";
    // <summary>Значение заголовка для челенджа.</summary>
    public const string ShiftHeaderValue = "301 Cancelled";
}
