using System;
using System.IO;
using System.Text;
using System.Threading;
using Microsoft.Extensions.Logging;

namespace SearchEngine.Api.Logger;

/// <summary>
/// Файловый логгер
/// </summary>
[Obsolete("use Serilog instead")]
internal class FileLoggerInternal : ILogger
{
    private readonly string _filePath;

    private readonly string _categoryName;

    private static readonly Lock Lock = new();

    internal FileLoggerInternal(string path, string? categoryName)
    {
        _filePath = path;
        _categoryName = categoryName ?? "";
    }

    /// <inheritdoc/>
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull
    {
        return null;
    }

    /// <inheritdoc/>
    public bool IsEnabled(LogLevel logLevel)
    {
        return true;
    }

    /// <inheritdoc/>
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
        Func<TState, Exception, string>? formatter)
    {
        if (formatter == null)
        {
            return;
        }

        var log = new StringBuilder();

        log.Append(DateTime.Now + "   [" + logLevel.ToString().ToUpperInvariant() + "] [" + _categoryName + "]  ");

        log.Append(formatter(state, exception!) + Environment.NewLine);

        if (exception != null)
        {
            log.Append(exception.GetType() + "    " + exception.Message + Environment.NewLine);
            log.Append(exception.StackTrace + Environment.NewLine);
        }

        var fullLog = log.ToString();

        lock (Lock)
        {
            File.AppendAllText(_filePath, fullLog);
        }
    }
}
