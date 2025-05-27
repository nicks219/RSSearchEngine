using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Npgsql;
using SearchEngine.Api.Services;
using SearchEngine.Api.Startup;
using SearchEngine.Service.Configuration;
using SearchEngine.Tooling.Contracts;
using SearchEngine.Tooling.Scripts;

namespace SearchEngine.Tooling.MigrationAssistant;

/// <summary>
/// Функционал работы с миграциями MySql.
/// </summary>
/// <param name="configuration">Конфигурация.</param>
public class NpgsqlDbMigrator(
    IConfiguration configuration,
    ILogger<NpgsqlDbMigrator> logger,
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

    private readonly CancellationToken _noneToken = CancellationToken.None;

    private int _version;

    /// <inheritdoc/>
    public async Task<string> Create(string? fileName, CancellationToken stoppingToken)
    {
        if (stoppingToken.IsCancellationRequested) return string.Empty;

        logger.LogInformation("[{Reporter}] | {Method} start", nameof(NpgsqlDbMigrator), nameof(Create));

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

        // путь к архиву можно отдавать только при создании zip - что тогда отдавать в режиме files-only?
        var destinationArchiveFileName = "dump files created";

        try
        {
            migratorState.Start();
            await using var connection = new NpgsqlConnection(connectionString);

            await connection.OpenAsync(stoppingToken);
            await using (var cmd = new NpgsqlCommand(NpgsqlScript.CreateDdl, connection))
            {
                var tablesDdl = new List<string>();
                await using var reader = cmd.ExecuteReader();
                while (await reader.ReadAsync(stoppingToken))
                {
                    tablesDdl.Add(reader.GetString(0));
                }

                var allTablesDdl = string.Join("\n\n", tablesDdl);
                await File.WriteAllTextAsync($"{backupFilesPath}{NpgsqlDdlSuffix}", allTablesDdl, stoppingToken);
                await File.WriteAllTextAsync($"{archiveTempPath}{NpgsqlDdlSuffix}", allTablesDdl, stoppingToken);
            }

            using (var tagToNotesReader =
                   await connection.BeginTextExportAsync("COPY public.\"TagsToNotes\"(\"TagId\", \"NoteId\") TO STDOUT",
                       stoppingToken))
            {
                var allTagToNotes = await tagToNotesReader.ReadToEndAsync(stoppingToken);
                await File.WriteAllTextAsync($"{backupFilesPath}{NpgsqlRelationsSuffix}", allTagToNotes, stoppingToken);
                await File.WriteAllTextAsync($"{archiveTempPath}{NpgsqlRelationsSuffix}", allTagToNotes, stoppingToken);
            }

            using (var notesReader =
                   await connection.BeginTextExportAsync(
                       "COPY public.\"Note\"(\"NoteId\", \"Title\", \"Text\") TO STDOUT", stoppingToken))
            {
                var allNotes = await notesReader.ReadToEndAsync(stoppingToken);
                await File.WriteAllTextAsync($"{backupFilesPath}{NpgsqlNotesSuffix}", allNotes, stoppingToken);
                await File.WriteAllTextAsync($"{archiveTempPath}{NpgsqlNotesSuffix}", allNotes, stoppingToken);
            }

            using (var tagsReader =
                   await connection.BeginTextExportAsync("COPY public.\"Tag\"(\"TagId\", \"Tag\") TO STDOUT",
                       stoppingToken))
            {
                var allTags = await tagsReader.ReadToEndAsync(stoppingToken);
                await File.WriteAllTextAsync($"{backupFilesPath}{NpgsqlTagsSuffix}", allTags, stoppingToken);
                await File.WriteAllTextAsync($"{archiveTempPath}{NpgsqlTagsSuffix}", allTags, stoppingToken);
            }

            using (var usersReader =
                   await connection.BeginTextExportAsync(
                       "COPY public.\"Users\"(\"Id\", \"Email\", \"Password\") TO STDOUT", stoppingToken))
            {
                var allUsers = await usersReader.ReadToEndAsync(stoppingToken);
                await File.WriteAllTextAsync($"{backupFilesPath}{NpgsqlUsersSuffix}", allUsers, stoppingToken);
                await File.WriteAllTextAsync($"{archiveTempPath}{NpgsqlUsersSuffix}", allUsers, stoppingToken);
            }

            // NB: при вызове на создании заметки будут созданы незаархивированные файлы
            if (IsCreateZippedDumpMode(fileName))
            {
                destinationArchiveFileName = GetArchiveFileName();
                File.Delete(destinationArchiveFileName);
                ZipFile.CreateFromDirectory(ArchiveTempDirectory, destinationArchiveFileName);
            }

        }
        catch (Exception ex)
        {
            logger.LogError("[{Reporter}] | {Method} error: '{Message}'", nameof(NpgsqlDbMigrator), nameof(Create), ex.Message);
        }
        finally
        {
            CleanUpTempDirectory(archiveTempPath);
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
    public async Task<string> Restore(string? fileName, CancellationToken stoppingToken)
    {
        if (stoppingToken.IsCancellationRequested) return string.Empty;

        logger.LogInformation("[{Reporter}] | {Method} start", nameof(NpgsqlDbMigrator), nameof(Restore));

        var connectionString = configuration.GetConnectionString(Startup.AdditionalConnectionKey);

        // в архиве всегда снимок последнего дампа
        var archiveTempPath = string.IsNullOrEmpty(fileName)
            ? Path.Combine(ArchiveTempDirectory, $"{NpgsqlDumpPrefix}_backup_.txt")
            : Path.Combine(ArchiveTempDirectory, $"{NpgsqlDumpPrefix}_{fileName}_.txt");

        var sourceArchiveFileName = GetArchiveFileName();

        try
        {
            migratorState.Start();
            ZipFile.ExtractToDirectory(sourceArchiveFileName, ArchiveTempDirectory);

            var allTablesDdl = await File.ReadAllTextAsync($"{archiveTempPath}{NpgsqlDdlSuffix}", stoppingToken);
            var allTagToNotes = await File.ReadAllTextAsync($"{archiveTempPath}{NpgsqlRelationsSuffix}", stoppingToken);
            var allNotes = await File.ReadAllTextAsync($"{archiveTempPath}{NpgsqlNotesSuffix}", stoppingToken);
            var allTags = await File.ReadAllTextAsync($"{archiveTempPath}{NpgsqlTagsSuffix}", stoppingToken);
            var allUsers = await File.ReadAllTextAsync($"{archiveTempPath}{NpgsqlUsersSuffix}", stoppingToken);

            await using var connection = new NpgsqlConnection(connectionString);
            await connection.OpenAsync(stoppingToken);
            await using var transaction = await connection.BeginTransactionAsync(stoppingToken);

            await using var cmd = connection.CreateCommand();
            cmd.Connection = connection;
            cmd.CommandText = allTablesDdl;
            cmd.Transaction = transaction;
            var rows = await cmd.ExecuteNonQueryAsync(stoppingToken);
            logger.LogInformation("[{Reporter}] | {Method} apply ddl | rows affected: '{Rows}'", nameof(NpgsqlDbMigrator),
                nameof(Restore), rows);

            await using (var notesWriter =
                         await connection.BeginTextImportAsync(
                             "COPY public.\"Note\"(\"NoteId\", \"Title\", \"Text\") FROM STDIN", stoppingToken))
            {
                await notesWriter.WriteAsync(allNotes);
            }

            await using (var tagsWriter =
                         await connection.BeginTextImportAsync("COPY public.\"Tag\"(\"TagId\", \"Tag\") FROM STDIN",
                             stoppingToken))
            {
                await tagsWriter.WriteAsync(allTags);
            }

            await using (var tagToNotesWriter =
                         await connection.BeginTextImportAsync(
                             "COPY public.\"TagsToNotes\"(\"TagId\", \"NoteId\") FROM STDIN", stoppingToken))
            {
                await tagToNotesWriter.WriteAsync(allTagToNotes);
            }

            await using (var usersWriter =
                         await connection.BeginTextImportAsync(
                             "COPY public.\"Users\"(\"Id\", \"Email\", \"Password\") FROM STDIN", stoppingToken))
            {
                await usersWriter.WriteAsync(allUsers);
            }

            await SetVals(connection, transaction, stoppingToken);

            await transaction.CommitAsync(_noneToken);
        }
        finally
        {
            CleanUpTempDirectory(archiveTempPath);
            migratorState.End();
        }

        return sourceArchiveFileName;
    }

    /// <inheritdoc/>
    public Task CopyDbFromMysqlToNpgsql(CancellationToken stoppingToken) =>
        throw new NotSupportedException($"use {nameof(MySqlDbMigrator)} instead.");

    // <summary/> выставить актуальные значения ключей
    private async Task SetVals(NpgsqlConnection connection, NpgsqlTransaction transaction, CancellationToken stoppingToken)
    {
        logger.LogInformation("[{Reporter}] | {Method} started", nameof(NpgsqlDbMigrator), nameof(SetVals));

        var commands = new List<string>
        {
            """SELECT setval(pg_get_serial_sequence('"Note"', 'NoteId'),(SELECT MAX("NoteId") FROM "Note"));""",
            """SELECT setval(pg_get_serial_sequence('"Tag"', 'TagId'),(SELECT MAX("TagId") FROM "Tag"));""",
            """SELECT setval(pg_get_serial_sequence('"Users"', 'Id'),(SELECT MAX("Id") FROM "Users"));"""
        };
        foreach (var command in commands)
        {
            await using var cmd = new NpgsqlCommand(command, connection, transaction);
            await cmd.ExecuteNonQueryAsync(stoppingToken);
        }
    }
}
