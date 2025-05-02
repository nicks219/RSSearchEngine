using System;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.DependencyInjection;
using SearchEngine.Domain.Configuration;

namespace SearchEngine.Api.Startup;

/// <summary>
/// Расширение для функционала рейтлимитера.
/// </summary>
internal static class RateLimiterExtensions
{
    /// <summary>
    /// Зарегистрировать функционал рейтлимитера.
    /// </summary>
    /// <param name="services">коллекция служб</param>
    internal static void AddRateLimiterInternal(this IServiceCollection services)
    {
        services.AddRateLimiter(rateLimiterOptions =>
        {
            rateLimiterOptions.RejectionStatusCode = 429;
            rateLimiterOptions.AddFixedWindowLimiter(policyName: Constants.MetricsHandlerPolicy, options =>
            {
                options.PermitLimit = 2;
                options.Window = TimeSpan.FromSeconds(20);
                options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                options.QueueLimit = 1;
            });
        });

    }
}
