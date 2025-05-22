using System;
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
    public static void CreateAndSeed(IServiceScopeFactory factory, ILogger logger)
    {
        // чтобы не закрывать контекст в корневом scope провайдера
        using var scope = factory.CreateScope();
        var mysqlContext = scope.ServiceProvider.GetRequiredService<MysqlCatalogContext>();
        var npgsqlContext = scope.ServiceProvider.GetRequiredService<NpgsqlCatalogContext>();

        try
        {
            var readerCreated = mysqlContext.Database.EnsureCreated();
            var writerCreated = npgsqlContext.Database.EnsureCreated();

            SeedDatabase(mysqlContext, readerCreated);
            SeedDatabase(npgsqlContext, writerCreated);

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
    private static void SeedDatabase(BaseCatalogContext context, bool created)
    {
        var database = context.Database;
        switch (database.ProviderName)
        {
            case "Npgsql.EntityFrameworkCore.PostgreSQL":
                if (created)
                {
                    var raws = database.ExecuteSqlRaw(NpgsqlScript.CreateUserOnlyData);
                    Console.WriteLine($"[Npgsql] [ROWS AFFECTED] {raws}");
                }

                break;

            case "Pomelo.EntityFrameworkCore.MySql":
                if (created)
                {
                    var raws = database.ExecuteSqlRaw(MySqlScript.CreateStubData);
                    Console.WriteLine($"[MySql] [ROWS AFFECTED] {raws}");
                }

                break;

            // SQLite используется при запуске интеграционных тестов:
            case "Microsoft.EntityFrameworkCore.Sqlite":

                if (!created)
                {
                    database.EnsureDeleted();
                    created = database.EnsureCreated();
                }

                if (created)
                {
                    var rows = database.ExecuteSqlRaw(SQLiteIntegrationTestScript.CreateTestData);
                    Console.WriteLine($"[Sqlite] [ROWS AFFECTED] {rows}");
                }

                break;

            case "Microsoft.EntityFrameworkCore.SqlServer":
                if (created)
                {
                    var raws = database.ExecuteSqlRaw(MsSqlScript.CreateStubData);
                    Console.WriteLine($"[SqlServer] [ROWS AFFECTED] {raws}");
                }

                break;
        }
    }
}
