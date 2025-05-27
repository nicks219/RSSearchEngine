using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SearchEngine.Exceptions;

namespace SearchEngine.Api.Middleware;

/// <summary>
/// Компонент централизованной обработки исключений.
/// </summary>
public class ExceptionHandlingMiddleware(
    RequestDelegate next,
    ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task Invoke(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (RsseBaseException ex)
        {
            context.Response.StatusCode = ex switch
            {
                RsseUserNotFoundException or RsseInvalidCredosException => StatusCodes.Status401Unauthorized,
                RsseDataExistsException or RsseInvalidDataException => StatusCodes.Status409Conflict,
                _ => context.Response.StatusCode
            };
        }
        catch (OperationCanceledException)
        {
            context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
        }
        catch (Exception ex)
        {
            var endpoint = context.GetEndpoint();
            var routePattern = endpoint?.DisplayName;
            logger.LogError(ex, "{Reporter} | unhandled exception: '{RoutePattern}'", nameof(ExceptionHandlingMiddleware), routePattern);
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        }
    }
}
