using MySql.Data.MySqlClient;

namespace RandomSongSearchEngine.Infrastructure;

public class MysqlBackup : IMysqlBackup
{
    private const string Directory = "Backup";
    private readonly IConfiguration _configuration;
    private readonly int _maxVersion;
    private int _version;

    public MysqlBackup(IConfiguration configuration)
    {
        _configuration = configuration;
        _maxVersion = 10;
    }
    
    public string Backup(string? fileName)
    {
        var connectionString = _configuration.GetConnectionString("DefaultConnection");
        
        var file = string.IsNullOrEmpty(fileName) 
            ? Path.Combine(Directory, $"backup_{_version}.sql")
            : Path.Combine(Directory, $"_{fileName}_.sql");

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
            ? Path.Combine(Directory, $"backup_{version}.sql")
            : Path.Combine(Directory, $"_{fileName}_.sql");

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