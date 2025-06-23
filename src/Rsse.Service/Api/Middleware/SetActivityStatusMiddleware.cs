using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace SearchEngine.Api.Middleware;

/// <summary>
/// Компонент выставления статуса для трасс.
/// </summary>
public class SetActivityStatusMiddleware(RequestDelegate next)
{
    /// <summary>
    /// Выставить статус в начале ответа, будет выставлен в тч при ошибке авторизации.
    /// </summary>
    /// <param name="context">Контекст.</param>
    public async Task InvokeAsync(HttpContext context)
    {
        context.Response.OnStarting(() =>
        {
            var activity = Activity.Current;
            if (activity == null)
            {
                return Task.CompletedTask;
            }

            var statusCode = context.Response.StatusCode;
            var status = statusCode >= 400 ? ActivityStatusCode.Error : ActivityStatusCode.Ok;
            activity.SetStatus(status);
            return Task.CompletedTask;
        });

        await next(context);
    }
}

/*await next();
// При ошибке авторизации статус выставлен не будет.
var activity = Activity.Current;
if (activity != null)
{
    var statusCode = context.Response.StatusCode;
    var status = statusCode >= 400 ? ActivityStatusCode.Error : ActivityStatusCode.Ok;
    activity.SetStatus(status);
}*/
