using System;
using Microsoft.Extensions.Logging;

namespace SearchEngine.Api.Logger;

/// <summary>
/// Провайдер файлового логера.
/// </summary>
[Obsolete("use Serilog instead")]
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
        return new FileLoggerInternal(_path, categoryName);
    }

    /// <inheritdoc/>
    void IDisposable.Dispose()
    {
        GC.SuppressFinalize(this);
    }
}
