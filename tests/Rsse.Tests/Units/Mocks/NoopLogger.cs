using System;
using Microsoft.Extensions.Logging;

namespace SearchEngine.Tests.Units.Mocks;

/// <summary/> Для тестов
public class NoopLogger<TModel> : ILogger<TModel>
{
    internal string? Message { get; private set; } = string.Empty;
    internal volatile bool Reported;

    public IDisposable BeginScope<TState>(TState state) where TState : notnull => throw new NotImplementedException();

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception, string> formatter)
    {
        Message = state?.ToString();
        Reported = true;
    }
}
