using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SearchEngine.Infrastructure.Context;
using SearchEngine.Tooling.Scripts;

namespace SearchEngine.Tooling.MigrationAssistant;

/// <summary>
/// Создание и первоначальное заполнение базы данных.
/// Требуется для интеграционных тестов и первоначального запуска до накатки дампа.
/// </summary>
public abstract class DatabaseInitializer
{
    /// <summary>
    /// Инициализировать две базы данных, вызывается однократно.
    /// </summary>
    public static async Task CreateAndSeedAsync(IServiceScopeFactory factory, ILogger logger, CancellationToken ct)
    {
        // чтобы не закрывать контекст в корневом scope провайдера
        using var scope = factory.CreateScope();
        var mysqlContext = scope.ServiceProvider.GetRequiredService<MysqlCatalogContext>();
        var npgsqlContext = scope.ServiceProvider.GetRequiredService<NpgsqlCatalogContext>();

        try
        {
            logger.LogInformation("[{Name}] started", nameof(DatabaseInitializer));

            // Исходим из того, что бд 'tagit' уже существует, например, создана при инициализации контейнера.
            await SeedDatabase(mysqlContext, ct);
            await SeedDatabase(npgsqlContext, ct);

            logger.LogInformation("[{Name}] finished", nameof(DatabaseInitializer));
        }
        catch (Exception ex)
        {
            logger.LogError("[{Reporter}] error | source: {Source} | ensure created error: '{Message}'",
                nameof(DatabaseInitializer), ex.Source, ex.Message);
        }
    }

    /// <summary>
    /// Заполнить базу тестовыми данными для SqLite либо данными изначальной авторизации для Postgres|MySql.
    /// </summary>
    /// <param name="context">Контекст базы данных.</param>
    /// <param name="ct">Токен отмены.</param>
    private static async Task SeedDatabase(BaseCatalogContext context, CancellationToken ct)
    {
        int rows;
        var database = context.Database;
        switch (database.ProviderName)
        {
            case "Npgsql.EntityFrameworkCore.PostgreSQL":
                rows = await database.ExecuteSqlRawAsync(NpgsqlScript.DdlData, ct);
                Console.WriteLine($"[{nameof(NpgsqlScript)}] rows affected | {rows}");
                break;

            case "Pomelo.EntityFrameworkCore.MySql":
                rows = await database.ExecuteSqlRawAsync(MySqlScript.DdlData, ct);
                Console.WriteLine($"[{nameof(MySqlScript)}] rows affected | {rows}");
                break;

            // Контекст абстрагирует SQLite, который используется при запуске интеграционных тестов:
            case "Microsoft.EntityFrameworkCore.Sqlite":
                var created = await database.EnsureCreatedAsync(ct);
                if (!created)
                {
                    await database.EnsureDeletedAsync(ct);
                    created = await database.EnsureCreatedAsync(ct);
                }

                if (created)
                {
                    rows = await database.ExecuteSqlRawAsync(SqLiteIntegrationTestScript.TestData, ct);
                    Console.WriteLine($"[{nameof(SqLiteIntegrationTestScript)}] rows affected | {rows}");
                }

                break;

            case "Microsoft.EntityFrameworkCore.SqlServer":
                rows = await database.ExecuteSqlRawAsync(MsSqlScript.TestData, ct);
                Console.WriteLine($"[{nameof(MsSqlScript)}] rows affected | {rows}");
                break;
        }
    }
}
