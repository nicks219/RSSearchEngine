using System;
using Microsoft.Extensions.Logging;

namespace SearchEngine.Common.Logger;

/// <summary>
/// Провайдер файлового логера
/// </summary>
internal class FileLoggerProvider : ILoggerProvider
{
    private readonly string _path;

    internal FileLoggerProvider(string path)
    {
        _path = path;
    }

    /// <inheritdoc/>
    public ILogger CreateLogger(string categoryName)
    {
        return new FileLogger(_path, categoryName);
    }

    /// <inheritdoc/>
    void IDisposable.Dispose()
    {
        GC.SuppressFinalize(this);
    }
}
