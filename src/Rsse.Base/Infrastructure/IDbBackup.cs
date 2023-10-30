namespace SearchEngine.Infrastructure;

public interface IDbBackup
{
    public string Backup(string? fileName);

    public string Restore(string? fileName);
}
