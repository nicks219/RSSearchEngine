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
            logger.LogDebug("{Type} | {Source} | {Message}", typeof(RsseBaseException), ex.Source, ex.Message);
            context.Response.StatusCode = ex switch
            {
                RsseUserNotFoundException or RsseInvalidCredosException => StatusCodes.Status401Unauthorized,
                RsseDataExistsException or RsseInvalidDataException => StatusCodes.Status409Conflict,
                _ => context.Response.StatusCode
            };
        }
        catch (OperationCanceledException ex)
        {
            logger.LogTrace("{Type} | {Source}", typeof(OperationCanceledException), ex.Source);
            context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
        }
        catch (Exception ex)
        {
            var endpoint = context.GetEndpoint();
            var routePattern = endpoint?.DisplayName;
            switch (ex.Source)
            {
                // Исключения при отсутствии коннекта до бд:
                // Postgres: InvalidOperationException: "An exception has been raised that is likely due to a transient failure."
                // MySql: MySqlException: "Unable to connect to any of the specified MySQL hosts"

                case "MySql.Data":
                case "Npgsql.EntityFrameworkCore.PostgreSQL":
                    logger.LogError("{Reporter} | unhandled exception from: '{RoutePattern}' | type: '{Type}' " +
                                    "| source: '{Source}' | message: '{Message}'",
                        nameof(ExceptionHandlingMiddleware), routePattern, ex.GetType().Name, ex.Source, ex.Message);
                    break;

                default:
                    logger.LogError(ex, "{Reporter} | unhandled exception from: '{RoutePattern}'",
                        nameof(ExceptionHandlingMiddleware), routePattern);
                    break;
            }

            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        }
    }
}
