using System.IO;
using System.IO.Compression;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace SearchEngine.Tools.MigrationAssistant;

public class NpgsqlDbMigrator(IConfiguration configuration) : IDbMigrator
{
    // todo
    // 1. проверить в docker
    // 2. использовать асинхронные перегрузки методов
    // 3. можно добавить количество заметок в именование архива
    // 4. варианты: либо склеить таблицы в файл, используя разделитель

    private const string NpgsqlDumpPrefix = "pg";
    private const string NpgsqlNotesSuffix = "notes";
    private const string NpgsqlTagsSuffix = "tags";
    private const string NpgsqlRelationsSuffix = "rel";
    private const string Directory = "ClientApp/build";
    private const int MaxVersion = 10;
    // что будет в docker?
    private const string ArchiveDirectory = "ClientApp/build/dump";
    private int _version;

    /// <inheritdoc/>
    public string Create(string? fileName)
    {
        var connectionString = configuration.GetConnectionString(Startup.AdditionalConnectionKey);

        var dumpFilesCoreName = string.IsNullOrEmpty(fileName)
            ? Path.Combine(Directory, $"{NpgsqlDumpPrefix}_backup_{_version}.txt")
            : Path.Combine(Directory, $"{NpgsqlDumpPrefix}_{fileName}_.txt");
        var archiveFilesCoreName = string.IsNullOrEmpty(fileName)
            ? Path.Combine(ArchiveDirectory, $"{NpgsqlDumpPrefix}_backup_{_version}.txt")
            : Path.Combine(ArchiveDirectory, $"{NpgsqlDumpPrefix}_{fileName}_.txt");

        if (IsCreateDumpMode(fileName))
        {
            _version = (_version + 1) % MaxVersion;
        }

        using var connection = new NpgsqlConnection(connectionString);

        connection.Open();
        using (var tagToNotesReader = connection.BeginTextExport("COPY public.\"TagsToNotes\"(\"TagId\", \"NoteId\") TO STDOUT"))
        {
            var allTagToNotes = tagToNotesReader.ReadToEnd();
            File.WriteAllText($"{dumpFilesCoreName}{NpgsqlRelationsSuffix}", allTagToNotes);
            File.WriteAllText($"{archiveFilesCoreName}{NpgsqlRelationsSuffix}", allTagToNotes);
        }

        using (var notesReader = connection.BeginTextExport("COPY public.\"Note\"(\"NoteId\", \"Title\", \"Text\") TO STDOUT"))
        {
            var allNotes = notesReader.ReadToEnd();
            File.WriteAllText($"{dumpFilesCoreName}{NpgsqlNotesSuffix}", allNotes);
            File.WriteAllText($"{archiveFilesCoreName}{NpgsqlNotesSuffix}", allNotes);
        }

        using (var notesReader = connection.BeginTextExport("COPY public.\"Tag\"(\"TagId\", \"Tag\") TO STDOUT"))
        {
            var allTags = notesReader.ReadToEnd();
            File.WriteAllText($"{dumpFilesCoreName}{NpgsqlTagsSuffix}", allTags);
            File.WriteAllText($"{archiveFilesCoreName}{NpgsqlTagsSuffix}", allTags);
        }

        connection.Close();

        var destinationArchiveFileName = GetArchiveFileName();
        // if presents, clean-up zipped archive at first:
        File.Delete(destinationArchiveFileName);

        try
        {
            if (IsCreateDumpMode(fileName))
            {
                ZipFile.CreateFromDirectory(ArchiveDirectory, destinationArchiveFileName);
            }
        }
        finally
        {
            CleanUpTempDirectory(archiveFilesCoreName);
        }

        return destinationArchiveFileName;

        bool IsCreateDumpMode(string? name) => string.IsNullOrEmpty(name);
    }

    private static string GetArchiveFileName() => Path.Combine(Directory, "dump.zip");
    private static void CleanUpTempDirectory(string archiveFilesCoreName)
    {
        File.Delete($"{archiveFilesCoreName}{NpgsqlRelationsSuffix}");
        File.Delete($"{archiveFilesCoreName}{NpgsqlNotesSuffix}");
        File.Delete($"{archiveFilesCoreName}{NpgsqlTagsSuffix}");
    }

    /// <inheritdoc/>
    public string Restore(string? fileName)
    {
        var connectionString = configuration.GetConnectionString(Startup.AdditionalConnectionKey);

        var version = _version - 1;

        if (version < 0)
        {
            version = MaxVersion - 1;
        }

        // при ресторе сразу после запуска я могу рассчитывать на "_maxVersion - 1" версию файлов:
        var dumpFilesCoreName = string.IsNullOrEmpty(fileName)
            ? Path.Combine(Directory, $"{NpgsqlDumpPrefix}_backup_{version}.txt")
            : Path.Combine(Directory, $"{NpgsqlDumpPrefix}_{fileName}_.txt");
        var archiveFilesCoreName = string.IsNullOrEmpty(fileName)
            ? Path.Combine(ArchiveDirectory, $"{NpgsqlDumpPrefix}_backup_{version}.txt")
            : Path.Combine(ArchiveDirectory, $"{NpgsqlDumpPrefix}_{fileName}_.txt");

        var sourceArchiveFileName = GetArchiveFileName();
        try
        {
            ZipFile.ExtractToDirectory(sourceArchiveFileName, ArchiveDirectory);

            using var connection = new NpgsqlConnection(connectionString);

            var allTagToNotes = File.ReadAllText($"{archiveFilesCoreName}{NpgsqlRelationsSuffix}");
            var allNotes = File.ReadAllText($"{dumpFilesCoreName}{NpgsqlNotesSuffix}");
            var allTags = File.ReadAllText($"{dumpFilesCoreName}{NpgsqlTagsSuffix}");

            connection.Open();

            using var cmd = connection.CreateCommand();
            cmd.Connection = connection;
            cmd.CommandText = """
                              TRUNCATE public."TagsToNotes" CASCADE;
                              TRUNCATE public."Tag" CASCADE;
                              TRUNCATE public."Note" CASCADE;
                              """;
            var rows = cmd.ExecuteNonQuery();

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

            connection.Close();
        }
        finally
        {
            CleanUpTempDirectory(archiveFilesCoreName);
        }

        return sourceArchiveFileName;
    }
}

