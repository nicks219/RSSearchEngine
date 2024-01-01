using Microsoft.Extensions.Logging;

namespace SearchEngine.Common.Logger;

/// <summary>
/// Расширение для функционала логера
/// </summary>
public static class FileLoggerExtensions
{
    /// <summary>
    /// Сконфигурировать логер возможностью писать в файл
    /// </summary>
    /// <param name="factory">фабрика</param>
    /// <param name="filePath">путь к файлу</param>
    public static void AddFile(this ILoggerFactory factory, string filePath)
    {
        factory.AddProvider(new FileLoggerProvider(filePath));
    }
}
