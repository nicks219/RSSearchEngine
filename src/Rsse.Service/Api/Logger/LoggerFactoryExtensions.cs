using System;
using Microsoft.Extensions.Logging;

namespace SearchEngine.Api.Logger;

/// <summary>
/// Расширение для функционала логера
/// </summary>
public static class LoggerFactoryExtensions
{
    /// <summary>
    /// Сконфигурировать логер возможностью писать в файл
    /// </summary>
    /// <param name="factory">фабрика</param>
    /// <param name="filePath">путь к файлу</param>
    [Obsolete("use Serilog instead")]
    public static void AddFileLoggerProviderInternal(this ILoggerFactory factory, string filePath)
    {
        factory.AddProvider(new FileLoggerProvider(filePath));
    }
}
