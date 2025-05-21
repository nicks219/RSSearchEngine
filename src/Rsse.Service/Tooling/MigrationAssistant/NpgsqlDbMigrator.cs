using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using SearchEngine.Api.Startup;
using SearchEngine.Domain.Configuration;
using SearchEngine.Infrastructure.Context;
using SearchEngine.Tooling.Contracts;
using SearchEngine.Tooling.Scripts;
using Serilog;

namespace SearchEngine.Tooling.MigrationAssistant;

public class NpgsqlDbMigrator(IConfiguration configuration, IServiceProvider serviceProvider) : IDbMigrator
{
    // todo
    // 1. проверить в docker
    // 2. использовать асинхронные перегрузки методов
    // 3. можно добавить количество заметок в именование архива
    // 4. варианты: либо склеить таблицы в файл, используя разделитель

    private const string NpgsqlDumpPrefix = "pg";
    private const string NpgsqlDdlSuffix = "ddl";
    private const string NpgsqlNotesSuffix = "notes";
    private const string NpgsqlTagsSuffix = "tags";
    private const string NpgsqlRelationsSuffix = "rel";
    private const string NpgsqlUsersSuffix = "usr";
    private const string ArchiveDirectory = "ClientApp/build";
    private const int MaxVersion = 10;
    // что будет в docker?
    private const string ArchiveTempDirectory = "ClientApp/build/dump";
    private int _version;

    /// <inheritdoc/>
    public string Create(string? fileName)
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

        using var connection = new NpgsqlConnection(connectionString);

        connection.Open();
        using (var cmd = new NpgsqlCommand(NpgsqlScript.CreateDdl, connection))
        {
            var tablesDdl = new List<string>();
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                tablesDdl.Add(reader.GetString(0));
            }

            var allTablesDdl = string.Join("\n\n", tablesDdl);
            File.WriteAllText($"{backupFilesPath}{NpgsqlDdlSuffix}", allTablesDdl);
            File.WriteAllText($"{archiveTempPath}{NpgsqlDdlSuffix}", allTablesDdl);
        }

        using (var tagToNotesReader = connection.BeginTextExport("COPY public.\"TagsToNotes\"(\"TagId\", \"NoteId\") TO STDOUT"))
        {
            var allTagToNotes = tagToNotesReader.ReadToEnd();
            File.WriteAllText($"{backupFilesPath}{NpgsqlRelationsSuffix}", allTagToNotes);
            File.WriteAllText($"{archiveTempPath}{NpgsqlRelationsSuffix}", allTagToNotes);
        }

        using (var notesReader = connection.BeginTextExport("COPY public.\"Note\"(\"NoteId\", \"Title\", \"Text\") TO STDOUT"))
        {
            var allNotes = notesReader.ReadToEnd();
            File.WriteAllText($"{backupFilesPath}{NpgsqlNotesSuffix}", allNotes);
            File.WriteAllText($"{archiveTempPath}{NpgsqlNotesSuffix}", allNotes);
        }

        using (var tagsReader = connection.BeginTextExport("COPY public.\"Tag\"(\"TagId\", \"Tag\") TO STDOUT"))
        {
            var allTags = tagsReader.ReadToEnd();
            File.WriteAllText($"{backupFilesPath}{NpgsqlTagsSuffix}", allTags);
            File.WriteAllText($"{archiveTempPath}{NpgsqlTagsSuffix}", allTags);
        }

        using (var usersReader = connection.BeginTextExport("COPY public.\"Users\"(\"Id\", \"Email\", \"Password\") TO STDOUT"))
        {
            var allUsers = usersReader.ReadToEnd();
            File.WriteAllText($"{backupFilesPath}{NpgsqlUsersSuffix}", allUsers);
            File.WriteAllText($"{archiveTempPath}{NpgsqlUsersSuffix}", allUsers);
        }

        connection.Close();

        // путь к архиву можно отдавать только при создании zip - что тогда отдавать в режиме files-only?
        var destinationArchiveFileName = "dump files created";

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
    public string Restore(string? fileName)
    {
        Log.Information("pg migrator on restore");

        var connectionString = configuration.GetConnectionString(Startup.AdditionalConnectionKey);

        // в архиве всегда снимок последнего дампа
        var archiveTempPath = string.IsNullOrEmpty(fileName)
            ? Path.Combine(ArchiveTempDirectory, $"{NpgsqlDumpPrefix}_backup_.txt")
            : Path.Combine(ArchiveTempDirectory, $"{NpgsqlDumpPrefix}_{fileName}_.txt");

        var sourceArchiveFileName = GetArchiveFileName();
        try
        {
            ZipFile.ExtractToDirectory(sourceArchiveFileName, ArchiveTempDirectory);

            // завязываемся на настройках
            var createTablesOnPgMigration = configuration.GetValue<bool>("DatabaseOptions:CreateTablesOnPgMigration");
            // пересоздадим базу до открытия соединения
            if (!createTablesOnPgMigration)
            {
                var context = serviceProvider.CreateScope().ServiceProvider.GetRequiredService<NpgsqlCatalogContext>();
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();
                context.Dispose();
                Log.Information("pg on restore | recreate database");
            }

            using var connection = new NpgsqlConnection(connectionString);

            var allTablesDdl = File.ReadAllText($"{archiveTempPath}{NpgsqlDdlSuffix}");
            var allTagToNotes = File.ReadAllText($"{archiveTempPath}{NpgsqlRelationsSuffix}");
            var allNotes = File.ReadAllText($"{archiveTempPath}{NpgsqlNotesSuffix}");
            var allTags = File.ReadAllText($"{archiveTempPath}{NpgsqlTagsSuffix}");
            var allUsers = File.ReadAllText($"{archiveTempPath}{NpgsqlUsersSuffix}");

            connection.Open();

            // завязываемся на настройках
            if (createTablesOnPgMigration)
            {
                using var cmd = connection.CreateCommand();
                cmd.Connection = connection;
                cmd.CommandText = allTablesDdl;
                var rows = cmd.ExecuteNonQuery();
                Log.Debug("pg on restore | apply ddl : '{CreateTable}' | rows affected: '{Rows}'", createTablesOnPgMigration, rows);
            }

            using (var notesWriter =
                   connection.BeginTextImport("COPY public.\"Note\"(\"NoteId\", \"Title\", \"Text\") FROM STDIN"))
            {
                notesWriter.Write(allNotes);
            }

            using (var tagsWriter = connection.BeginTextImport("COPY public.\"Tag\"(\"TagId\", \"Tag\") FROM STDIN"))
            {
                tagsWriter.Write(allTags);
            }

            using (var tagToNotesWriter =
                   connection.BeginTextImport("COPY public.\"TagsToNotes\"(\"TagId\", \"NoteId\") FROM STDIN"))
            {
                tagToNotesWriter.Write(allTagToNotes);
            }

            using (var usersWriter =
                   connection.BeginTextImport("COPY public.\"Users\"(\"Id\", \"Email\", \"Password\") FROM STDIN"))
            {
                usersWriter.Write(allUsers);
            }

            SetVals(connection);

            connection.Close();
        }
        finally
        {
            CleanUpTempDirectory(archiveTempPath);
        }

        return sourceArchiveFileName;
    }

    /// <inheritdoc/>
    public Task CopyDbFromMysqlToNpgsql() => throw new NotSupportedException($"use {nameof(MySqlDbMigrator)} instead.");

    // <summary/> выставить актуальные значения ключей
    private static void SetVals(NpgsqlConnection connection)
    {
        var commands = new List<string>
        {
            """SELECT setval(pg_get_serial_sequence('"Note"', 'NoteId'),(SELECT MAX("NoteId") FROM "Note"));""",
            """SELECT setval(pg_get_serial_sequence('"Tag"', 'TagId'),(SELECT MAX("TagId") FROM "Tag"));""",
            """SELECT setval(pg_get_serial_sequence('"Users"', 'Id'),(SELECT MAX("Id") FROM "Users"));"""
        };
        foreach (var command in commands)
        {
            using var cmd = new NpgsqlCommand(command, connection);
            cmd.ExecuteNonQuery();
        }
    }
}
