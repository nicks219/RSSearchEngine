namespace SearchEngine.Data.Configuration;

/// <summary>
/// Константы для данных
/// </summary>
public static class CommonDataConstants
{
    /// <summary>
    /// Максимальная длина текста в символах, дефолт 4000
    /// </summary>
    public const int MaxTextLength = 5000;

    /// <summary>
    /// Максимальная длина названия в символах, дефолт 50
    /// </summary>
    public const int MaxTitleLength = 50;

    /// <summary>
    /// Email для логина, используется только в инициализирующем скрипте
    /// </summary>
    public const string Email = "1@2";

    /// <summary>
    /// Пароль для логина, используется только в инициализирующем скрипте
    /// </summary>
    public const string Password = "12";
}
