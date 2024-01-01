using System;
using Microsoft.Extensions.Logging;

namespace SearchEngine.Tests.Units.Mocks;

internal class TestLogger<TModel> : ILogger<TModel>
{
    internal string? ErrorMessage { get; private set; } = string.Empty;

    public IDisposable BeginScope<TState>(TState state) where TState : notnull => throw new NotImplementedException();

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception, string> formatter)
    {
        ErrorMessage = state?.ToString();
    }
}
