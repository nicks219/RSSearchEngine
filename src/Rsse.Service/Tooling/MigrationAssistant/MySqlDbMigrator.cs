using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;
using SearchEngine.Api.Services;
using SearchEngine.Api.Startup;
using SearchEngine.Infrastructure.Context;
using SearchEngine.Services.Configuration;
using SearchEngine.Tooling.Contracts;

namespace SearchEngine.Tooling.MigrationAssistant;

/// <summary>
/// Функционал работы с миграциями MySql.
/// </summary>
/// <param name="configuration">Конфигурация.</param>
/// <param name="factory">Из фабрики однократко получаем контексты бд для копирования данных.</param>
internal class MySqlDbMigrator(
    IConfiguration configuration,
    ILogger<MySqlDbMigrator> logger,
    IServiceScopeFactory factory,
    MigratorState migratorState) : IDbMigrator
{
    private const int MaxVersion = 10;
    private readonly CancellationToken _noneToken = CancellationToken.None;
    private int _version;
    private int _perSongVersion;

    /// <inheritdoc/>
    public async Task<string> Create(string? fileName, CancellationToken stoppingToken)
    {
        if (stoppingToken.IsCancellationRequested) return string.Empty;

        logger.LogInformation("[{Reporter}] | {Method} started", nameof(MySqlDbMigrator), nameof(Create));

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
        await conn.OpenAsync(stoppingToken);

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
    public async Task<string> Restore(string? fileName, CancellationToken stoppingToken)
    {
        if (stoppingToken.IsCancellationRequested) return string.Empty;

        logger.LogInformation("[{Reporter}] | {Method} started", nameof(MySqlDbMigrator), nameof(Restore));

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

        await conn.OpenAsync(stoppingToken);

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
    public async Task CopyDbFromMysqlToNpgsql(CancellationToken stoppingToken)
    {
        if (stoppingToken.IsCancellationRequested) return;
        logger.LogInformation("[{Reporter}] | {Method} started", nameof(MySqlDbMigrator), nameof(CopyDbFromMysqlToNpgsql));

        using var scope = factory.CreateScope();
        var mysqlCatalogContext = (MysqlCatalogContext)scope.ServiceProvider.GetRequiredService(typeof(MysqlCatalogContext));
        var npgsqlCatalogContext = (NpgsqlCatalogContext)scope.ServiceProvider.GetRequiredService(typeof(NpgsqlCatalogContext));

        try
        {
            migratorState.Start();
            await CopyDbFromMysqlToNpgsql(mysqlCatalogContext, npgsqlCatalogContext, stoppingToken);
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
        CancellationToken stoppingToken)
    {
        if (mysqlCatalogContext == null || npgsqlCatalogContext == null)
            throw new InvalidOperationException(
                $"{nameof(MySqlDbMigrator)} | {nameof(CopyDbFromMysqlToNpgsql)} | null context(s).");

        // AddRangeAsync вместе с таблицей Notes подхватит селектнутые отношения, на это поведение нельзя полагаться
        var notes = await mysqlCatalogContext.Notes
            .AsNoTracking()
            .ToListAsync(cancellationToken: stoppingToken);

        var tags = await mysqlCatalogContext.Tags
            .AsNoTracking()
            .ToListAsync(cancellationToken: stoppingToken);

        var tagsToNotes = await mysqlCatalogContext.TagsToNotesRelation
            .AsNoTracking()
            .ToListAsync(cancellationToken: stoppingToken);

        var users = await mysqlCatalogContext.Users
            .AsNoTracking()
            .ToListAsync(cancellationToken: stoppingToken);

        await using var transaction = await npgsqlCatalogContext.Database.BeginTransactionAsync(stoppingToken);

        // очищаем таблицы postgres:
        await CleanUpDb(npgsqlCatalogContext, stoppingToken);

        // notes, tags, relations:
        await npgsqlCatalogContext.Notes.AddRangeAsync(notes, stoppingToken);
        await npgsqlCatalogContext.Tags.AddRangeAsync(tags, stoppingToken);
        await npgsqlCatalogContext.TagsToNotesRelation.AddRangeAsync(tagsToNotes, stoppingToken);

        // users:
        await npgsqlCatalogContext.Users.ExecuteDeleteAsync(cancellationToken: stoppingToken);
        await npgsqlCatalogContext.Users.AddRangeAsync(users, stoppingToken);

        await npgsqlCatalogContext.SaveChangesAsync(stoppingToken);

        // мы заполнили значение ключей "вручную" и EF не изменил identity
        await PgSetVals(npgsqlCatalogContext, stoppingToken);

        await transaction.CommitAsync(_noneToken);
    }

    /// <summary>
    /// Очистить таблицы Postgres.
    /// </summary>
    /// <param name="dbContext">Контекст, представляющий доступ к бд.</param>
    /// <param name="stoppingToken">Токен отмены.</param>
    private async Task CleanUpDb(BaseCatalogContext dbContext, CancellationToken stoppingToken)
    {
        CheckIfProviderSupported(dbContext);

        logger.LogInformation("[{Reporter}] | {Method} started", nameof(MySqlDbMigrator), nameof(CleanUpDb));
        var commands = new List<string>
        {
            """TRUNCATE TABLE "Users" RESTART IDENTITY CASCADE;""",
            """TRUNCATE TABLE "TagsToNotes" RESTART IDENTITY CASCADE;""",
            """TRUNCATE TABLE "Tag" RESTART IDENTITY CASCADE;""",
            """TRUNCATE TABLE "Note" RESTART IDENTITY CASCADE;"""
        };
        foreach (var command in commands)
        {
            await dbContext.Database.ExecuteSqlRawAsync(command, cancellationToken: stoppingToken);
        }
    }

    /// <summary>
    /// Выставить актуальные значения ключей для Postgres.
    /// </summary>
    private async Task PgSetVals(BaseCatalogContext dbContext, CancellationToken stoppingToken)
    {
        CheckIfProviderSupported(dbContext);

        logger.LogInformation("[{Reporter}] | {Method} started", nameof(MySqlDbMigrator), nameof(CleanUpDb));
        var commands = new List<string>
        {
            """SELECT setval(pg_get_serial_sequence('"Note"', 'NoteId'),(SELECT MAX("NoteId") FROM "Note"));""",
            """SELECT setval(pg_get_serial_sequence('"Tag"', 'TagId'),(SELECT MAX("TagId") FROM "Tag"));""",
            """SELECT setval(pg_get_serial_sequence('"Users"', 'Id'),(SELECT MAX("Id") FROM "Users"));"""
        };
        foreach (var command in commands)
        {
            await dbContext.Database.ExecuteSqlRawAsync(command, cancellationToken: stoppingToken);
        }
    }

    /// <summary>
    /// Удостовериться в том, что провайдер бд поддерживается.
    /// </summary>
    /// <param name="dbContext">Контекст бд.</param>
    /// <exception cref="NotSupportedException">Провайдер не поддерживается.</exception>
    private static void CheckIfProviderSupported(BaseCatalogContext dbContext)
    {
        if (dbContext.Database.ProviderName != "Npgsql.EntityFrameworkCore.PostgreSQL")
        {
            throw new NotSupportedException(
                $"[{nameof(CheckIfProviderSupported)}] | '{dbContext.Database.ProviderName}' provider is not supported.");
        }
    }
}

