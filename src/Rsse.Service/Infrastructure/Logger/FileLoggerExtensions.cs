using Microsoft.Extensions.Logging;

namespace SearchEngine.Infrastructure.Logger;

public static class FileLoggerExtensions
{
    public static ILoggerFactory AddFile(this ILoggerFactory factory, string filePath)
    {
        factory.AddProvider(new FileLoggerProvider(filePath));

        return factory;
    }
}
