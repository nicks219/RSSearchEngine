namespace SearchEngine.Data.Configuration;

public static class CommonDataConstants
{
    // максимальная длина текста в символах, дефолт 4000:
    public const int MaxTextLength = 5000;

    // максимальная длина названия в символах, дефолт 50:
    public const int MaxTitleLength = 50;

    // email для логина, используется только в инициализирующем скрипте:
    public const string Email = "1@2";

    // пароль для логина, используется только в инициализирующем скрипте:
    public const string Password = "12";
}
