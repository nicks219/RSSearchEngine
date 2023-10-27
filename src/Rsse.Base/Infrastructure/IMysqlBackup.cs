namespace SearchEngine.Infrastructure;

public interface IMysqlBackup
{
    public string Backup(string? fileName);

    public string Restore(string? fileName);
}
