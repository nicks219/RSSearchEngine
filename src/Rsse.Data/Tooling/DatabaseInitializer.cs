using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SearchEngine.Infrastructure.Context;
using SearchEngine.Tooling.Scripts;

namespace SearchEngine.Tooling;

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
            var readerCreated = await mysqlContext.Database.EnsureCreatedAsync(ct);
            var writerCreated = await npgsqlContext.Database.EnsureCreatedAsync(ct);

            await SeedDatabase(mysqlContext, readerCreated, ct);
            await SeedDatabase(npgsqlContext, writerCreated, ct);

            logger.LogInformation("[{Name}] finished with results: {FirstResult} - {SecondResult}",
                nameof(DatabaseInitializer), readerCreated, writerCreated);
        }
        catch (Exception ex)
        {
            logger.LogError("[{Reporter}] error | source: {Source} | ensure created error: '{Message}'",
                nameof(DatabaseInitializer), ex.Source, ex.Message);
        }
    }

    /// <summary>
    /// Заполнить базу тестовыми данными.
    /// </summary>
    /// <param name="context">Контекст базы данных.</param>
    /// <param name="created">Была ли база создана.</param>
    /// <param name="ct"></param>
    private static async Task SeedDatabase(BaseCatalogContext context, bool created, CancellationToken ct)
    {
        var database = context.Database;
        switch (database.ProviderName)
        {
            case "Npgsql.EntityFrameworkCore.PostgreSQL":
                if (created)
                {
                    var raws = await database.ExecuteSqlRawAsync(NpgsqlScript.CreateUserOnlyData, ct);
                    Console.WriteLine($"[Npgsql] [ROWS AFFECTED] {raws}");
                }

                break;

            case "Pomelo.EntityFrameworkCore.MySql":
                if (created)
                {
                    var raws = await database.ExecuteSqlRawAsync(MySqlScript.CreateStubData, ct);
                    Console.WriteLine($"[MySql] [ROWS AFFECTED] {raws}");
                }

                break;

            // SQLite используется при запуске интеграционных тестов:
            case "Microsoft.EntityFrameworkCore.Sqlite":

                if (!created)
                {
                    await database.EnsureDeletedAsync(ct);
                    created = await database.EnsureCreatedAsync(ct);
                }

                if (created)
                {
                    var rows = await database.ExecuteSqlRawAsync(SQLiteIntegrationTestScript.CreateTestData, ct);
                    Console.WriteLine($"[Sqlite] [ROWS AFFECTED] {rows}");
                }

                break;

            case "Microsoft.EntityFrameworkCore.SqlServer":
                if (created)
                {
                    var raws = await database.ExecuteSqlRawAsync(MsSqlScript.CreateStubData, ct);
                    Console.WriteLine($"[SqlServer] [ROWS AFFECTED] {raws}");
                }

                break;
        }
    }
}
