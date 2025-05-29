using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace SearchEngine.Tests.Integrations.Extensions;

/// <summary>
/// Обертка для проверочных делегатов в тестах.
/// </summary>
internal static class Response
{
    /// <summary>
    /// Проверить ответ теста.
    /// </summary>
    /// <param name="validator">Метод с проверкой ответа.</param>
    internal static Func<HttpResponseMessage, Task> Validate(Func<HttpResponseMessage, Task> validator)
        => validator;

    /// <summary>
    /// Не проверять ответ теста.
    /// </summary>
    /// <returns>Проверка всегда успешна.</returns>
    internal static Func<HttpResponseMessage, Task> SkipValidation() => _ => Task.CompletedTask;
}
