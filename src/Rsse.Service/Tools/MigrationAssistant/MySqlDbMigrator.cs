using System.IO;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;

namespace SearchEngine.Tools.MigrationAssistant;

/// <summary>
/// Функционал работы с миграциями MySql
/// </summary>
internal class MySqlDbMigrator : IDbMigrator
{
    private const string Directory = "ClientApp/build";
    private readonly IConfiguration _configuration;
    private readonly int _maxVersion;
    private int _version;
    private int _perSongVersion;

    public MySqlDbMigrator(IConfiguration configuration)
    {
        _configuration = configuration;
        _maxVersion = 10;
    }

    /// <inheritdoc/>
    public string Create(string? fileName)
    {
        var connectionString = _configuration.GetConnectionString(Startup.DefaultConnectionKey);

        var fileWithPath = string.IsNullOrEmpty(fileName)
            ? Path.Combine(Directory, $"backup_{_version}.txt")
            : Path.Combine(Directory, $"_{fileName}_{_perSongVersion}.txt");

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

        conn.Close();

        return fileWithPath;

        // ротация счетчика версий:
        void IncrementVersion(ref int version) => version = (version + 1) % _maxVersion;
    }

    /// <inheritdoc/>
    public string Restore(string? fileName)
    {
        var connectionString = _configuration.GetConnectionString(Startup.DefaultConnectionKey);

        var version = _version - 1;

        if (version < 0)
        {
            version = _maxVersion - 1;
        }

        var fileWithPath = string.IsNullOrEmpty(fileName)
            ? Path.Combine(Directory, $"backup_{version}.txt")
            : Path.Combine(Directory, $"_{fileName}_.txt");

        using var conn = new MySqlConnection(connectionString);

        using var cmd = new MySqlCommand();

        using var mb = new MySqlBackup(cmd);

        cmd.Connection = conn;

        conn.Open();

        mb.ImportFromFile(fileWithPath);

        conn.Close();

        return fileWithPath;
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
