namespace SearchEngine.Common.Auth;

/// <summary>
/// Системные константы приложения.
/// </summary>
public abstract class Constants
{
    // <summary>Версия приложения.</summary>
    private const string ApplicationVersion = "5.2.6";
    // <summary>Версия API.</summary>
    internal const string ApiVersion = "5.1";
    // <summary>Полное название версии приложения.</summary>
    internal const string ApplicationFullName = $"v{ApplicationVersion}: .NET8/React18";
    // <summary>Именование документации OpenAPI, транслируется в сегмент пути к описанию.</summary>
    internal const string SwaggerDocNameSegment = "v1";
    // <summary>Именование заголовка Swagger.</summary>
    internal const string SwaggerTitle = "RSSearchEngine API";
    // <summary>Именование политики полного доступа.</summary>
    internal const string FullAccessPolicyName = nameof(FullAccessPolicyName);
    // <summary>Утверждение для проверки внутреннего идентификатора.</summary>
    internal const string IdInternalClaimType = nameof(IdInternalClaimType);
    // <summary>Идентификатор администратора.</summary>
    internal const string AdminId = "1";
}
