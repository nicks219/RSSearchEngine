using System.Text;

namespace RandomSongSearchEngine.Infrastructure.Logger;

/// <summary>
/// Мой логгер
/// </summary>
public class FileLogger : ILogger
{
    private readonly string _filePath;

    private readonly string _categoryName;

    private static readonly object Lock = new();

    public FileLogger(string path, string? categoryName)
    {
        _filePath = path;
        _categoryName = categoryName ?? "";
    }

    public IDisposable BeginScope<TState>(TState state)
    {
        return null!;
    }

    //TODO доделай лог левел
    public bool IsEnabled(LogLevel logLevel)
    {
        //return logLevel == LogLevel.Trace;
        return true;
    }

    /// <summary>
    /// Метод формирует текст лога и записывает его в файл
    /// </summary>
    /// <typeparam name="TState"></typeparam>
    /// <param name="logLevel"></param>
    /// <param name="eventId"></param>
    /// <param name="state"></param>
    /// <param name="exception"></param>
    /// <param name="formatter"></param>
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
