using System;
using static Rsse.Domain.Service.Configuration.Constants;

namespace Rsse.Domain.Service.Configuration;

/// <summary>
/// Проверка окружения.
/// </summary>
public static class EnvironmentReporter
{
    /// <summary>
    /// Вернуть признак запуска в производственном окружении.
    /// </summary>
    public static bool IsProduction()
    {
        var environment = Environment.GetEnvironmentVariable(AspNetCoreEnvironmentName)?.ToLower();
        return environment?.Equals(ProductionEnvironment, StringComparison.CurrentCultureIgnoreCase) ?? false;
    }

    /// <summary>
    /// Бросить исключение в производственном окружении.
    /// </summary>
    public static void ThrowIfProduction(string name)
    {
        var environment = Environment.GetEnvironmentVariable(AspNetCoreEnvironmentName)?.ToLower();
        var isProduction = environment?.Equals(ProductionEnvironment, StringComparison.CurrentCultureIgnoreCase) ?? false;
        if (isProduction)
        {
            throw new NotSupportedException($"[{name}] Is in development and not supported for production environment.");
        }
    }

    /// <summary>
    /// Вернуть признак запуска в окружении разработки.
    /// </summary>
    public static bool IsDevelopment()
    {
        var environment = Environment.GetEnvironmentVariable(AspNetCoreEnvironmentName)?.ToLower();
        return environment?.Equals(DevelopmentEnvironment, StringComparison.CurrentCultureIgnoreCase) ?? false;
    }

    /// <summary>
    /// Вернуть признак запуска для тестирования в переменной.
    /// </summary>
    public static bool CheckIfTesting(string environmentValue)
    {
        return string.Equals(environmentValue, TestingEnvironment, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Вернуть признак запуска для тестирования.
    /// </summary>
    public static bool IsTesting()
    {
        var environment = Environment.GetEnvironmentVariable(AspNetCoreEnvironmentName)?.ToLower();
        return environment?.Equals(TestingEnvironment, StringComparison.CurrentCultureIgnoreCase) ?? false;
    }
}
