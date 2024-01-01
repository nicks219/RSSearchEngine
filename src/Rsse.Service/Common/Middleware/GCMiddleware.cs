using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace SearchEngine.Common.Middleware;

/// <summary>
/// Настройка сборщика мусора для частого освобождения памяти
/// </summary>
internal class GcMiddleware
{
    private readonly RequestDelegate _next;

    internal GcMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    internal async Task Invoke(HttpContext httpContext)
    {
        await _next(httpContext);

        GC.Collect(2, GCCollectionMode.Forced, true);

        GC.WaitForPendingFinalizers();
    }
}
