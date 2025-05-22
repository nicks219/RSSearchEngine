namespace SearchEngine.Data.Configuration;

/// <summary>
/// Константы для данных.
/// </summary>
public static class CommonDataConstants
{
    /// <summary>
    /// Максимальная длина текста в символах.
    /// </summary>
    public const int MaxTextLength = 10000;

    /// <summary>
    /// Максимальная длина названия в символах.
    /// </summary>
    public const int MaxTitleLength = 50;

    /// <summary>
    /// Email для логина, используется только в инициализирующем скрипте MySql.
    /// </summary>
    public const string Email = "1@2";

    /// <summary>
    /// Пароль для логина, используется только в инициализирующем скрипте MySql.
    /// </summary>
    public const string Password = "12";
}
