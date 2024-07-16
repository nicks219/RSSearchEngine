using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SearchEngine.Controllers;
using SearchEngine.Data.Context;

namespace SearchEngine.Tests.Integrations.Infra;

/// <summary>
/// Расширение функционала регистрации служб, для целей тестирования.
/// </summary>
public static class TestConfigurationExtensions
{
    /// <summary>
    /// Зарегистрировать контроллеры и провайдер для тестовой бд.
    /// </summary>
    /// <param name="services">коллекция служб</param>
    internal static void PartialConfigureForTesting(this IServiceCollection services)
    {
        // todo разберись почему требуется AddApplicationPart:
        // https://andrewlock.net/when-asp-net-core-cant-find-your-controller-debugging-application-parts/

        services
            .AddControllers()
            .AddApplicationPart(typeof(ReadController).Assembly);

        // SQLite: https://learn.microsoft.com/en-us/ef/core/get-started/overview/first-app?tabs=netcore-cli
        // функциональность проверена на Windows/Ubuntu:

        const Environment.SpecialFolder folder = Environment.SpecialFolder.LocalApplicationData;
        var path = Environment.GetFolderPath(folder);
        var dbPath = System.IO.Path.Join(path, "testing-2.db");
        var connectionString = $"Data Source={dbPath}";

        services.AddDbContext<CatalogContext>(options => options.UseSqlite(connectionString));
    }
}
