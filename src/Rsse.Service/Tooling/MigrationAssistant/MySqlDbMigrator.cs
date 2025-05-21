using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MySql.Data.MySqlClient;
using SearchEngine.Api.Startup;
using SearchEngine.Domain.Configuration;
using SearchEngine.Domain.Contracts;
using SearchEngine.Tooling.Contracts;
using Serilog;

namespace SearchEngine.Tooling.MigrationAssistant;

/// <summary>
/// Функционал работы с миграциями MySql
/// </summary>
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

        conn.Close();

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

        conn.Close();

        return fileWithPath;
    }

    /// <inheritdoc/>
    public async Task CopyDbFromMysqlToNpgsql()
    {
        await using var repo = factory.CreateScope().ServiceProvider.GetRequiredService<IDataRepository>();
        await repo.CopyDbFromMysqlToNpgsql();
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
