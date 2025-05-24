using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MySql.Data.MySqlClient;
using SearchEngine.Api.Services;
using SearchEngine.Api.Startup;
using SearchEngine.Exceptions;
using SearchEngine.Infrastructure.Context;
using SearchEngine.Service.Configuration;
using SearchEngine.Tooling.Contracts;
using Serilog;

namespace SearchEngine.Tooling.MigrationAssistant;

/// <summary>
/// Функционал работы с миграциями MySql.
/// </summary>
/// <param name="configuration">Конфигурация.</param>
/// <param name="factory">Из фабрики однократко получаем контексты бд для копирования данных.</param>
internal class MySqlDbMigrator(
    IConfiguration configuration,
    IServiceScopeFactory factory,
    MigratorState migratorState) : IDbMigrator
{
    private const int MaxVersion = 10;
    private readonly CancellationToken _rollbackToken = CancellationToken.None;
    private int _version;
    private int _perSongVersion;

    /// <inheritdoc/>
    public async Task<string> Create(string? fileName, CancellationToken ct)
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

        await using var conn = new MySqlConnection(connectionString);

        await using var cmd = new MySqlCommand();

        using var mb = new MySqlBackup(cmd);

        cmd.Connection = conn;

        // уточнить необходимость открывать соединение перед вызовом import и отметить в комментарии
        await conn.OpenAsync(ct);

        try
        {
            migratorState.Start();
            mb.ExportToFile(fileWithPath);
        }
        finally
        {
            migratorState.End();
        }

        return fileWithPath;

        // ротация счетчика версий:
        void IncrementVersion(ref int version) => version = (version + 1) % MaxVersion;
    }

    /// <inheritdoc/>
    public async Task<string> Restore(string? fileName, CancellationToken ct)
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

        await using var conn = new MySqlConnection(connectionString);

        await using var cmd = new MySqlCommand();

        using var mb = new MySqlBackup(cmd);

        // уточнить необходимость открывать соединение перед вызовом import и отметить в комментарии
        cmd.Connection = conn;

        await conn.OpenAsync(ct);

        try
        {
            migratorState.Start();
            mb.ImportFromFile(fileWithPath);
        }
        finally
        {
            migratorState.End();
        }

        return fileWithPath;
    }

    /// <inheritdoc/>
    public async Task CopyDbFromMysqlToNpgsql(CancellationToken ct)
    {
        using var scope = factory.CreateScope();
        var mysqlCatalogContext = (MysqlCatalogContext)scope.ServiceProvider.GetRequiredService(typeof(MysqlCatalogContext));
        var npgsqlCatalogContext = (NpgsqlCatalogContext)scope.ServiceProvider.GetRequiredService(typeof(NpgsqlCatalogContext));
        try
        {
            migratorState.Start();
            await CopyDbFromMysqlToNpgsql(mysqlCatalogContext, npgsqlCatalogContext, ct);
        }
        finally
        {
            migratorState.End();
        }
    }

    /// <summary>
    /// Копировать данные из MySql в Postgres.
    /// </summary>
    private async Task CopyDbFromMysqlToNpgsql(
        MysqlCatalogContext mysqlCatalogContext,
        NpgsqlCatalogContext npgsqlCatalogContext,
        CancellationToken ct)
    {
        if (mysqlCatalogContext == null || npgsqlCatalogContext == null)
            throw new InvalidOperationException($"[Warning] {nameof(CopyDbFromMysqlToNpgsql)} | null context(s).");

        // AddRangeAsync вместе с таблицей Notes подхватит селектнутые отношения, на это поведение нельзя полагаться
        var notes = mysqlCatalogContext.Notes.Select(note => note).ToList();
        var tags = mysqlCatalogContext.Tags.Select(tag => tag).ToList();
        var tagsToNotes = mysqlCatalogContext.TagsToNotesRelation.Select(relation => relation).ToList();

        var users = mysqlCatalogContext.Users.Select(user => user).ToList();

        // пересоздаём базу перед копированием данных
        await npgsqlCatalogContext.Database.EnsureDeletedAsync(ct);
        await npgsqlCatalogContext.Database.EnsureCreatedAsync(ct);
        await using var transaction = await npgsqlCatalogContext.Database.BeginTransactionAsync(ct);

        try
        {
            // notes, tags, relations:
            await npgsqlCatalogContext.Notes.AddRangeAsync(notes, ct);
            await npgsqlCatalogContext.Tags.AddRangeAsync(tags, ct);
            await npgsqlCatalogContext.TagsToNotesRelation.AddRangeAsync(tagsToNotes, ct);

            // users:
            await npgsqlCatalogContext.Users.ExecuteDeleteAsync(cancellationToken: ct);
            await npgsqlCatalogContext.Users.AddRangeAsync(users, ct);

            await npgsqlCatalogContext.SaveChangesAsync(ct);

            // мы заполнили значение ключей "вручную" и EF не изменил identity
            await PgSetVals(npgsqlCatalogContext, ct);

            await transaction.CommitAsync(ct);
        }
        catch (Exception ex) when (ex is DataExistsException or OperationCanceledException)
        {
            await transaction.RollbackAsync(_rollbackToken);
        }
        catch (Exception ex)
        {
            // include error detail:
            await transaction.RollbackAsync(_rollbackToken);
            Console.WriteLine(ex.Message);
            throw new Exception($"[{nameof(CopyDbFromMysqlToNpgsql)}: Repo]", ex);
        }
    }

    /// <summary>
    /// Выставить актуальные значения ключей для postgres.
    /// </summary>
    private static async Task PgSetVals(BaseCatalogContext dbContext, CancellationToken ct)
    {
        if (dbContext.Database.ProviderName != "Npgsql.EntityFrameworkCore.PostgreSQL")
        {
            throw new NotSupportedException($"{nameof(PgSetVals)} | '{dbContext.Database.ProviderName}' provider is not supported.");
        }

        var noteRows = await dbContext.Database.ExecuteSqlRawAsync("""SELECT setval(pg_get_serial_sequence('"Note"', 'NoteId'),(SELECT MAX("NoteId") FROM "Note"));""", cancellationToken: ct);
        var tagRows = await dbContext.Database.ExecuteSqlRawAsync("""SELECT setval(pg_get_serial_sequence('"Tag"', 'TagId'),(SELECT MAX("TagId") FROM "Tag"));""", cancellationToken: ct);
        var userRows = await dbContext.Database.ExecuteSqlRawAsync("""SELECT setval(pg_get_serial_sequence('"Users"', 'Id'),(SELECT MAX("Id") FROM "Users"));""", cancellationToken: ct);
        Console.WriteLine($"Migrator set val | noteRows : {noteRows} | tagRows : {tagRows} | userRows : {userRows}");
    }
}

