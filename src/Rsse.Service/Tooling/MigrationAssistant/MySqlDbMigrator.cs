using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MySql.Data.MySqlClient;
using SearchEngine.Api.Startup;
using SearchEngine.Infrastructure.Context;
using SearchEngine.Infrastructure.Repository.Exceptions;
using SearchEngine.Service.Configuration;
using SearchEngine.Tooling.Contracts;
using Serilog;

namespace SearchEngine.Tooling.MigrationAssistant;

/// <summary>
/// Функционал работы с миграциями MySql.
/// </summary>
/// <param name="configuration">Конфигурация.</param>
/// <param name="factory">Из фабрики однократко получаем контексты бд для копирования данных.</param>
internal class MySqlDbMigrator(IConfiguration configuration, IServiceScopeFactory factory) : IDbMigrator
{
    private const int MaxVersion = 10;
    private int _version;
    private int _perSongVersion;

    /// <inheritdoc/>
    public string Create(string? fileName)
    {
        Log.Information("mysql migrator on create");

        var connectionString = configuration.GetConnectionString(Startup.DefaultConnectionKey);

        var fileWithPath = string.IsNullOrEmpty(fileName)
            ? Path.Combine(Constants.StaticDirectory, $"backup_{_version}{Constants.MySqlDumpExt}")
            : Path.Combine(Constants.StaticDirectory, $"_{fileName}_{_perSongVersion}{Constants.MySqlDumpExt}");

        IncrementVersion(
            ref string.IsNullOrEmpty(fileName)
            ? ref _version
            : ref _perSongVersion);

        using var conn = new MySqlConnection(connectionString);

        using var cmd = new MySqlCommand();

        using var mb = new MySqlBackup(cmd);

        cmd.Connection = conn;

        conn.Open();

        mb.ExportToFile(fileWithPath);

        return fileWithPath;

        // ротация счетчика версий:
        void IncrementVersion(ref int version) => version = (version + 1) % MaxVersion;
    }

    /// <inheritdoc/>
    public string Restore(string? fileName)
    {
        Log.Information("mysql migrator on restore");

        var connectionString = configuration.GetConnectionString(Startup.DefaultConnectionKey);

        var version = _version - 1;

        if (version < 0)
        {
            version = MaxVersion - 1;
        }

        var fileWithPath = string.IsNullOrEmpty(fileName)
            ? Path.Combine(Constants.StaticDirectory, $"backup_{version}{Constants.MySqlDumpExt}")
            : Path.Combine(Constants.StaticDirectory, $"_{fileName}_{Constants.MySqlDumpExt}");

        using var conn = new MySqlConnection(connectionString);

        using var cmd = new MySqlCommand();

        using var mb = new MySqlBackup(cmd);

        cmd.Connection = conn;

        conn.Open();

        mb.ImportFromFile(fileWithPath);

        return fileWithPath;
    }

    /// <inheritdoc/>
    public async Task CopyDbFromMysqlToNpgsql()
    {
        using var scope = factory.CreateScope();
        var mysqlCatalogContext = (MysqlCatalogContext)scope.ServiceProvider.GetRequiredService(typeof(MysqlCatalogContext));
        var npgsqlCatalogContext = (NpgsqlCatalogContext)scope.ServiceProvider.GetRequiredService(typeof(NpgsqlCatalogContext));
        await CopyDbFromMysqlToNpgsql(mysqlCatalogContext, npgsqlCatalogContext);
    }

    /// <summary>
    /// Копировать данные из MySql в Postgres.
    /// </summary>
    private async Task CopyDbFromMysqlToNpgsql(MysqlCatalogContext mysqlCatalogContext, NpgsqlCatalogContext npgsqlCatalogContext)
    {
        if (mysqlCatalogContext == null || npgsqlCatalogContext == null)
            throw new InvalidOperationException($"[Warning] {nameof(CopyDbFromMysqlToNpgsql)} | null context(s).");

        // пересоздаём базу перед копированием данных
        await npgsqlCatalogContext.Database.EnsureDeletedAsync();
        await npgsqlCatalogContext.Database.EnsureCreatedAsync();

        // AddRangeAsync вместе с таблицей Notes подхватит селектнутые отношения, на это поведение нельзя полагаться
        var notes = mysqlCatalogContext.Notes.Select(note => note).ToList();
        var tags = mysqlCatalogContext.Tags.Select(tag => tag).ToList();
        var tagsToNotes = mysqlCatalogContext.TagsToNotesRelation.Select(relation => relation).ToList();

        var users = mysqlCatalogContext.Users.Select(user => user).ToList();

        await using var transaction = await npgsqlCatalogContext.Database.BeginTransactionAsync();

        try
        {
            // notes, tags, relations:
            await npgsqlCatalogContext.Notes.AddRangeAsync(notes);
            await npgsqlCatalogContext.Tags.AddRangeAsync(tags);
            await npgsqlCatalogContext.TagsToNotesRelation.AddRangeAsync(tagsToNotes);

            // users:
            await npgsqlCatalogContext.Users.ExecuteDeleteAsync();
            await npgsqlCatalogContext.Users.AddRangeAsync(users);

            await npgsqlCatalogContext.SaveChangesAsync();

            // мы заполнили значение ключей "вручную" и EF не изменил identity
            await PgSetVals(npgsqlCatalogContext);

            await transaction.CommitAsync();
        }
        catch (DataExistsException)
        {
            await transaction.RollbackAsync();
        }
        catch (Exception ex)
        {
            // include error detail:
            await transaction.RollbackAsync();
            Console.WriteLine(ex.Message);
            throw new Exception($"[{nameof(CopyDbFromMysqlToNpgsql)}: Repo]", ex);
        }
    }

    /// <summary>
    /// Выставить актуальные значения ключей для postgres.
    /// </summary>
    private static async Task PgSetVals(BaseCatalogContext dbContext)
    {
        if (dbContext.Database.ProviderName != "Npgsql.EntityFrameworkCore.PostgreSQL")
        {
            throw new NotSupportedException($"{nameof(PgSetVals)} | '{dbContext.Database.ProviderName}' provider is not supported.");
        }

        var noteRows = await dbContext.Database.ExecuteSqlRawAsync("""SELECT setval(pg_get_serial_sequence('"Note"', 'NoteId'),(SELECT MAX("NoteId") FROM "Note"));""");
        var tagRows = await dbContext.Database.ExecuteSqlRawAsync("""SELECT setval(pg_get_serial_sequence('"Tag"', 'TagId'),(SELECT MAX("TagId") FROM "Tag"));""");
        var userRows = await dbContext.Database.ExecuteSqlRawAsync("""SELECT setval(pg_get_serial_sequence('"Users"', 'Id'),(SELECT MAX("Id") FROM "Users"));""");
        Console.WriteLine($"Migrator set val | noteRows : {noteRows} | tagRows : {tagRows} | userRows : {userRows}");
    }
}

// TODO: миграции можно реализовать средствами какой-либо утилиты, например:

// export DOTNET_ROLL_FORWARD=LatestMajor
// Microsoft.EntityFrameworkCore.Design
// dotnet new tool-manifest
// dotnet tool update dotnet-ef (7.0.1)
// dotnet ef dbcontext list
// dotnet ef migrations list
// создаём миграцию из папки RsseBase: dotnet ef migrations add Init -s "./" -p "../Rsse.Data"
