using System;
using Microsoft.Extensions.Logging;

namespace SearchEngine.Infrastructure.Logger;

public class FileLoggerProvider : ILoggerProvider
{
    private readonly string _path;

    public FileLoggerProvider(string path)
    {
        _path = path;
    }

    public ILogger CreateLogger(string categoryName)
    {
        return new FileLogger(_path, categoryName);
    }

    void IDisposable.Dispose()
    {
        GC.SuppressFinalize(this);
    }
}
