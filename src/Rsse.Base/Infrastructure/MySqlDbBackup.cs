using System.IO;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;

namespace SearchEngine.Infrastructure;

public class MySqlDbBackup : IDbBackup
{
    private const string Directory = "ClientApp/build";
    private readonly IConfiguration _configuration;
    private readonly int _maxVersion;
    private int _version;

    public MySqlDbBackup(IConfiguration configuration)
    {
        _configuration = configuration;
        _maxVersion = 10;
    }

    public string Backup(string? fileName)
    {
        var connectionString = _configuration.GetConnectionString("DefaultConnection");

        var file = string.IsNullOrEmpty(fileName)
            ? Path.Combine(Directory, $"backup_{_version}.txt")
            : Path.Combine(Directory, $"_{fileName}_.txt");

        _version = (_version + 1) % _maxVersion;

        using var conn = new MySqlConnection(connectionString);

        using var cmd = new MySqlCommand();

        using var mb = new MySqlBackup(cmd);

        cmd.Connection = conn;

        conn.Open();

        mb.ExportToFile(file);

        conn.Close();

        return file;
    }

    public string Restore(string? fileName)
    {
        var connectionString = _configuration.GetConnectionString("DefaultConnection");

        var version = _version - 1;

        if (version < 0)
        {
            version = _maxVersion - 1;
        }

        var file = string.IsNullOrEmpty(fileName)
            ? Path.Combine(Directory, $"backup_{version}.txt")
            : Path.Combine(Directory, $"_{fileName}_.txt");

        using var conn = new MySqlConnection(connectionString);

        using var cmd = new MySqlCommand();

        using var mb = new MySqlBackup(cmd);

        cmd.Connection = conn;

        conn.Open();

        mb.ImportFromFile(file);

        conn.Close();

        return file;
    }
}
