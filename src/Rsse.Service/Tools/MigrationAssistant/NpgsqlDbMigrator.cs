using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using Microsoft.Extensions.Configuration;
using Npgsql;
using SearchEngine.Data.Repository.Scripts;
using Serilog;

namespace SearchEngine.Tools.MigrationAssistant;

public class NpgsqlDbMigrator(IConfiguration configuration) : IDbMigrator
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

        if (IsCreateDumpMode(fileName))
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

        connection.Close();

        var destinationArchiveFileName = GetArchiveFileName();

        File.Delete(destinationArchiveFileName);

        try
        {
            if (IsCreateDumpMode(fileName))
            {
                ZipFile.CreateFromDirectory(ArchiveTempDirectory, destinationArchiveFileName);
            }
        }
        finally
        {
            CleanUpTempDirectory(archiveTempPath);
        }

        return destinationArchiveFileName;

        bool IsCreateDumpMode(string? name) => string.IsNullOrEmpty(name);
    }

    /// <summary/> Вернёт dump.zip в клиентской директории
    private static string GetArchiveFileName() => Path.Combine(ArchiveDirectory, "dump.zip");
    private static void CleanUpTempDirectory(string archiveTempPath)
    {
        File.Delete($"{archiveTempPath}{NpgsqlDdlSuffix}");
        File.Delete($"{archiveTempPath}{NpgsqlRelationsSuffix}");
        File.Delete($"{archiveTempPath}{NpgsqlNotesSuffix}");
        File.Delete($"{archiveTempPath}{NpgsqlTagsSuffix}");
    }

    /// <inheritdoc/>
    public string Restore(string? fileName)
    {
        Log.Information("pg migrator on restore");

        var connectionString = configuration.GetConnectionString(Startup.AdditionalConnectionKey);

        var version = _version - 1;

        if (version < 0)
        {
            version = MaxVersion - 1;
        }

        // при ресторе сразу после запуска я могу рассчитывать на "_maxVersion - 1" версию файлов:
        var backupFilesPath = string.IsNullOrEmpty(fileName)
            ? Path.Combine(ArchiveDirectory, $"{NpgsqlDumpPrefix}_backup_{version}.txt")
            : Path.Combine(ArchiveDirectory, $"{NpgsqlDumpPrefix}_{fileName}_.txt");

        // в архиве всегда снимок последнего дампа
        var archiveTempPath = string.IsNullOrEmpty(fileName)
            ? Path.Combine(ArchiveTempDirectory, $"{NpgsqlDumpPrefix}_backup_.txt")
            : Path.Combine(ArchiveTempDirectory, $"{NpgsqlDumpPrefix}_{fileName}_.txt");

        var sourceArchiveFileName = GetArchiveFileName();
        try
        {
            ZipFile.ExtractToDirectory(sourceArchiveFileName, ArchiveTempDirectory);

            using var connection = new NpgsqlConnection(connectionString);

            var allTablesDdl = File.ReadAllText($"{archiveTempPath}{NpgsqlDdlSuffix}");
            var allTagToNotes = File.ReadAllText($"{archiveTempPath}{NpgsqlRelationsSuffix}");
            var allNotes = File.ReadAllText($"{archiveTempPath}{NpgsqlNotesSuffix}");
            var allTags = File.ReadAllText($"{archiveTempPath}{NpgsqlTagsSuffix}");

            connection.Open();

            // завязываемся на настройках
            var createTablesOnPgMigration = configuration.GetValue<bool>("DatabaseOptions:CreateTablesOnPgMigration");
            var rows = 0;
            if (createTablesOnPgMigration)
            {
                using var cmd = connection.CreateCommand();
                cmd.Connection = connection;
                cmd.CommandText = allTablesDdl;
                rows = cmd.ExecuteNonQuery();
            }
            Log.Debug("pg restore apply ddl : '{CreateTable}' | rows affected: '{Rows}'", createTablesOnPgMigration, rows);

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

            SetVals(connection);

            connection.Close();
        }
        finally
        {
            CleanUpTempDirectory(archiveTempPath);
        }

        return sourceArchiveFileName;
    }

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
