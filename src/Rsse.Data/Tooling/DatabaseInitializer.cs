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
            var readerCreated = await mysqlContext.Database.EnsureCreatedAsync(ct).ConfigureAwait(false);
            var writerCreated = await npgsqlContext.Database.EnsureCreatedAsync(ct).ConfigureAwait(false);

            await SeedDatabase(mysqlContext, readerCreated, ct).ConfigureAwait(false);
            await SeedDatabase(npgsqlContext, writerCreated, ct).ConfigureAwait(false);

            logger.LogInformation("[{Name}] finished with results: {FirstResult} - {SecondResult}",
                nameof(DatabaseInitializer), readerCreated, writerCreated);
        }
        catch (Exception ex)
        {
            logger.LogError("[{Reporter}] error | source: {Source} | ensure created error: '{Message}'", nameof(DatabaseInitializer), ex.Source, ex.Message);
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
                    var raws = await database.ExecuteSqlRawAsync(NpgsqlScript.CreateUserOnlyData, ct)
                        .ConfigureAwait(false);
                    Console.WriteLine($"[Npgsql] [ROWS AFFECTED] {raws}");
                }

                break;

            case "Pomelo.EntityFrameworkCore.MySql":
                if (created)
                {
                    var raws = await database.ExecuteSqlRawAsync(MySqlScript.CreateStubData, ct)
                        .ConfigureAwait(false);
                    Console.WriteLine($"[MySql] [ROWS AFFECTED] {raws}");
                }

                break;

            // SQLite используется при запуске интеграционных тестов:
            case "Microsoft.EntityFrameworkCore.Sqlite":

                if (!created)
                {
                    await database.EnsureDeletedAsync(ct).ConfigureAwait(false);
                    created = await database.EnsureCreatedAsync(ct).ConfigureAwait(false);
                }

                if (created)
                {
                    var rows = await database.ExecuteSqlRawAsync(SQLiteIntegrationTestScript.CreateTestData, ct)
                        .ConfigureAwait(false);
                    Console.WriteLine($"[Sqlite] [ROWS AFFECTED] {rows}");
                }

                break;

            case "Microsoft.EntityFrameworkCore.SqlServer":
                if (created)
                {
                    var raws = await database.ExecuteSqlRawAsync(MsSqlScript.CreateStubData, ct)
                        .ConfigureAwait(false);
                    Console.WriteLine($"[SqlServer] [ROWS AFFECTED] {raws}");
                }

                break;
        }
    }
}
