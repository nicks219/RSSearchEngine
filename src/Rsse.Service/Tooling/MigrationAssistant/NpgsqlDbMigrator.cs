using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using SearchEngine.Api.Services;
using SearchEngine.Api.Startup;
using SearchEngine.Infrastructure.Context;
using SearchEngine.Service.Configuration;
using SearchEngine.Tooling.Contracts;
using SearchEngine.Tooling.Scripts;
using Serilog;

namespace SearchEngine.Tooling.MigrationAssistant;

/// <summary>
/// Функционал работы с миграциями MySql.
/// </summary>
/// <param name="configuration">Конфигурация.</param>
/// <param name="serviceProvider">Из провайдера однократно получаем NpgsqlCatalogContext для пересоздания бд.</param>
public class NpgsqlDbMigrator(
    IConfiguration configuration,
    IServiceProvider serviceProvider,
    MigratorState migratorState) : IDbMigrator
{
    private const string NpgsqlDumpPrefix = "pg";
    private const string NpgsqlDdlSuffix = "ddl";
    private const string NpgsqlNotesSuffix = "notes";
    private const string NpgsqlTagsSuffix = "tags";
    private const string NpgsqlRelationsSuffix = "rel";
    private const string NpgsqlUsersSuffix = "usr";
    private const string ArchiveDirectory = "ClientApp/build";
    private const int MaxVersion = 10;
    private const string ArchiveTempDirectory = "ClientApp/build/dump";

    private readonly CancellationToken _rollbackToken = CancellationToken.None;

    private int _version;

    /// <inheritdoc/>
    public async Task<string> Create(string? fileName, CancellationToken ct)
    {
        Log.Information("pg migrator on create");

        var connectionString = configuration.GetConnectionString(Startup.AdditionalConnectionKey);

        // файлы с версиями являются "историей" дампов, создание дампа также может запросить CreateNoteAndDumpAsync
        var backupFilesPath = string.IsNullOrEmpty(fileName)
            ? Path.Combine(ArchiveDirectory, $"{NpgsqlDumpPrefix}_backup_{_version}.txt")
            : Path.Combine(ArchiveDirectory, $"{NpgsqlDumpPrefix}_{fileName}_.txt");
        // архив самого последнего созданного дампа, нет смысла обогащать содержащиеся файлы версией
        var archiveTempPath = string.IsNullOrEmpty(fileName)
            ? Path.Combine(ArchiveTempDirectory, $"{NpgsqlDumpPrefix}_backup_.txt")
            : Path.Combine(ArchiveTempDirectory, $"{NpgsqlDumpPrefix}_{fileName}_.txt");

        Directory.CreateDirectory(ArchiveTempDirectory);

        if (IsCreateZippedDumpMode(fileName))
        {
            _version = (_version + 1) % MaxVersion;
        }

        NpgsqlTransaction? transaction = null;
        // путь к архиву можно отдавать только при создании zip - что тогда отдавать в режиме files-only?
        var destinationArchiveFileName = "dump files created";
        try
        {
            migratorState.Start();
            await using var connection = new NpgsqlConnection(connectionString);

            await connection.OpenAsync(ct);
            transaction = await connection.BeginTransactionAsync(ct);
            await using (var cmd = new NpgsqlCommand(NpgsqlScript.CreateDdl, connection))
            {
                var tablesDdl = new List<string>();
                await using var reader = cmd.ExecuteReader();
                while (await reader.ReadAsync(ct))
                {
                    tablesDdl.Add(reader.GetString(0));
                }

                var allTablesDdl = string.Join("\n\n", tablesDdl);
                await File.WriteAllTextAsync($"{backupFilesPath}{NpgsqlDdlSuffix}", allTablesDdl, ct);
                await File.WriteAllTextAsync($"{archiveTempPath}{NpgsqlDdlSuffix}", allTablesDdl, ct);
            }

            using (var tagToNotesReader =
                   await connection.BeginTextExportAsync("COPY public.\"TagsToNotes\"(\"TagId\", \"NoteId\") TO STDOUT",
                       ct))
            {
                var allTagToNotes = await tagToNotesReader.ReadToEndAsync(ct);
                await File.WriteAllTextAsync($"{backupFilesPath}{NpgsqlRelationsSuffix}", allTagToNotes, ct);
                await File.WriteAllTextAsync($"{archiveTempPath}{NpgsqlRelationsSuffix}", allTagToNotes, ct);
            }

            using (var notesReader =
                   await connection.BeginTextExportAsync(
                       "COPY public.\"Note\"(\"NoteId\", \"Title\", \"Text\") TO STDOUT", ct))
            {
                var allNotes = await notesReader.ReadToEndAsync(ct);
                await File.WriteAllTextAsync($"{backupFilesPath}{NpgsqlNotesSuffix}", allNotes, ct);
                await File.WriteAllTextAsync($"{archiveTempPath}{NpgsqlNotesSuffix}", allNotes, ct);
            }

            using (var tagsReader =
                   await connection.BeginTextExportAsync("COPY public.\"Tag\"(\"TagId\", \"Tag\") TO STDOUT", ct))
            {
                var allTags = await tagsReader.ReadToEndAsync(ct);
                await File.WriteAllTextAsync($"{backupFilesPath}{NpgsqlTagsSuffix}", allTags, ct);
                await File.WriteAllTextAsync($"{archiveTempPath}{NpgsqlTagsSuffix}", allTags, ct);
            }

            using (var usersReader =
                   await connection.BeginTextExportAsync(
                       "COPY public.\"Users\"(\"Id\", \"Email\", \"Password\") TO STDOUT", ct))
            {
                var allUsers = await usersReader.ReadToEndAsync(ct);
                await File.WriteAllTextAsync($"{backupFilesPath}{NpgsqlUsersSuffix}", allUsers, ct);
                await File.WriteAllTextAsync($"{archiveTempPath}{NpgsqlUsersSuffix}", allUsers, ct);
            }

            try
            {
                // NB: при вызове на создании заметки будут созданы незаархивированные файлы
                if (IsCreateZippedDumpMode(fileName))
                {
                    destinationArchiveFileName = GetArchiveFileName();
                    File.Delete(destinationArchiveFileName);
                    ZipFile.CreateFromDirectory(ArchiveTempDirectory, destinationArchiveFileName);
                }
            }
            finally
            {
                CleanUpTempDirectory(archiveTempPath);
            }

            await transaction.CommitAsync(ct);
        }
        catch
        {
            if (transaction != null)
            {
                await transaction.RollbackAsync(_rollbackToken);
            }
        }
        finally
        {
            if (transaction != null)
            {
                await transaction.DisposeAsync();
            }

            migratorState.End();
        }

        return destinationArchiveFileName;

        bool IsCreateZippedDumpMode(string? name) => string.IsNullOrEmpty(name);
    }

    /// <summary/> Вернёт dump.zip в клиентской директории
    private static string GetArchiveFileName() => Path.Combine(ArchiveDirectory, Constants.PostgresDumpArchiveName);
    private static void CleanUpTempDirectory(string archiveTempPath)
    {
        File.Delete($"{archiveTempPath}{NpgsqlDdlSuffix}");
        File.Delete($"{archiveTempPath}{NpgsqlRelationsSuffix}");
        File.Delete($"{archiveTempPath}{NpgsqlNotesSuffix}");
        File.Delete($"{archiveTempPath}{NpgsqlTagsSuffix}");
        File.Delete($"{archiveTempPath}{NpgsqlUsersSuffix}");
    }

    /// <inheritdoc/>
    public async Task<string> Restore(string? fileName, CancellationToken ct)
    {
        Log.Information("pg migrator on restore");

        var connectionString = configuration.GetConnectionString(Startup.AdditionalConnectionKey);

        // в архиве всегда снимок последнего дампа
        var archiveTempPath = string.IsNullOrEmpty(fileName)
            ? Path.Combine(ArchiveTempDirectory, $"{NpgsqlDumpPrefix}_backup_.txt")
            : Path.Combine(ArchiveTempDirectory, $"{NpgsqlDumpPrefix}_{fileName}_.txt");

        var sourceArchiveFileName = GetArchiveFileName();

        NpgsqlTransaction? transaction = null;
        try
        {
            migratorState.Start();
            ZipFile.ExtractToDirectory(sourceArchiveFileName, ArchiveTempDirectory);

            // завязываемся на настройках
            var createTablesOnPgMigration = configuration.GetValue<bool>("DatabaseOptions:CreateTablesOnPgMigration");
            // пересоздадим базу до открытия соединения
            if (!createTablesOnPgMigration)
            {
                using var scope = serviceProvider.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<NpgsqlCatalogContext>();
                await context.Database.EnsureDeletedAsync(ct);
                await context.Database.EnsureCreatedAsync(ct);
                Log.Information("pg on restore | recreate database");
            }

            var allTablesDdl = await File.ReadAllTextAsync($"{archiveTempPath}{NpgsqlDdlSuffix}", ct);
            var allTagToNotes = await File.ReadAllTextAsync($"{archiveTempPath}{NpgsqlRelationsSuffix}", ct);
            var allNotes = await File.ReadAllTextAsync($"{archiveTempPath}{NpgsqlNotesSuffix}", ct);
            var allTags = await File.ReadAllTextAsync($"{archiveTempPath}{NpgsqlTagsSuffix}", ct);
            var allUsers = await File.ReadAllTextAsync($"{archiveTempPath}{NpgsqlUsersSuffix}", ct);

            await using var connection = new NpgsqlConnection(connectionString);
            await connection.OpenAsync(ct);
            transaction = await connection.BeginTransactionAsync(ct);

            // завязываемся на настройках
            if (createTablesOnPgMigration)
            {
                await using var cmd = connection.CreateCommand();
                cmd.Connection = connection;
                cmd.CommandText = allTablesDdl;
                var rows = await cmd.ExecuteNonQueryAsync(ct);
                Log.Debug("pg on restore | apply ddl : '{CreateTable}' | rows affected: '{Rows}'", createTablesOnPgMigration, rows);
            }

            await using (var notesWriter =
                         await connection.BeginTextImportAsync("COPY public.\"Note\"(\"NoteId\", \"Title\", \"Text\") FROM STDIN", ct))
            {
                await notesWriter.WriteAsync(allNotes);
            }

            await using (var tagsWriter = await connection.BeginTextImportAsync("COPY public.\"Tag\"(\"TagId\", \"Tag\") FROM STDIN", ct))
            {
                await tagsWriter.WriteAsync(allTags);
            }

            await using (var tagToNotesWriter =
                         await connection.BeginTextImportAsync("COPY public.\"TagsToNotes\"(\"TagId\", \"NoteId\") FROM STDIN", ct))
            {
                await tagToNotesWriter.WriteAsync(allTagToNotes);
            }

            await using (var usersWriter =
                         await connection.BeginTextImportAsync("COPY public.\"Users\"(\"Id\", \"Email\", \"Password\") FROM STDIN", ct))
            {
                await usersWriter.WriteAsync(allUsers);
            }

            await SetVals(connection, ct);
            await transaction.CommitAsync(ct);
        }
        catch
        {
            if (transaction != null)
            {
                await transaction.RollbackAsync(_rollbackToken);
            }
        }
        finally
        {
            if (transaction != null)
            {
                await transaction.DisposeAsync();
            }

            migratorState.End();
            CleanUpTempDirectory(archiveTempPath);
        }

        return sourceArchiveFileName;
    }

    /// <inheritdoc/>
    public Task CopyDbFromMysqlToNpgsql(CancellationToken _) => throw new NotSupportedException($"use {nameof(MySqlDbMigrator)} instead.");

    // <summary/> выставить актуальные значения ключей
    private static async Task SetVals(NpgsqlConnection connection, CancellationToken ct)
    {
        var commands = new List<string>
        {
            """SELECT setval(pg_get_serial_sequence('"Note"', 'NoteId'),(SELECT MAX("NoteId") FROM "Note"));""",
            """SELECT setval(pg_get_serial_sequence('"Tag"', 'TagId'),(SELECT MAX("TagId") FROM "Tag"));""",
            """SELECT setval(pg_get_serial_sequence('"Users"', 'Id'),(SELECT MAX("Id") FROM "Users"));"""
        };
        foreach (var command in commands)
        {
            await using var cmd = new NpgsqlCommand(command, connection);
            await cmd.ExecuteNonQueryAsync(ct);
        }
    }
}
