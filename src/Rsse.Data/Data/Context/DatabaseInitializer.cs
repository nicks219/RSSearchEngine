using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SearchEngine.Data.Repository.Contracts;
using SearchEngine.Data.Repository.Scripts;

namespace SearchEngine.Data.Context;

/// <summary>
/// Создание и заполнение базы данных.
/// Требуется для интеграционных тестов и первоначального запуска до накатки дампа.
/// </summary>
public abstract class DatabaseInitializer
{
    /// <summary>
    /// Инициализировать базы данных, вызывается однократно.
    /// </summary>
    public static void CreateAndSeed(IServiceProvider provider, ILogger logger)
    {
        // чтобы не закрывать контекст в корневом scope провайдера
        using var repo = provider.CreateScope().ServiceProvider.GetRequiredService<IDataRepository>();

        try
        {
            var readerContext = repo.GetReaderContext();
            var primaryWriterContext = repo.GetPrimaryWriterContext();

            if (readerContext == null || primaryWriterContext == null)
            {
                logger.LogWarning("[{Reporter}] | No context(s) provided", nameof(DatabaseInitializer));
                return;
            }

            var readerCreated = readerContext.Database.EnsureCreated();
            var writerCreated = primaryWriterContext.Database.EnsureCreated();

            SeedDatabase(readerContext, readerCreated);
            SeedDatabase(primaryWriterContext, writerCreated);

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
    /// <param name="context">контекст базы данных</param>
    /// <param name="created">была ли база создана</param>
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
