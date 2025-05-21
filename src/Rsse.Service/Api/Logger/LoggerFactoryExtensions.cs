using System;
using Microsoft.Extensions.Logging;

namespace SearchEngine.Api.Logger;

/// <summary>
/// Расширение для функционала логера.
/// </summary>
public static class LoggerFactoryExtensions
{
    /// <summary>
    /// Добавить в фабрику логеров файловый провайдером.
    /// </summary>
    /// <param name="factory">Фабрика логеров.</param>
    /// <param name="filePath">Путь к файлу.</param>
    [Obsolete("use Serilog instead")]
    public static void AddFileLoggerProviderInternal(this ILoggerFactory factory, string filePath)
    {
        factory.AddProvider(new FileLoggerProvider(filePath));
    }
}
