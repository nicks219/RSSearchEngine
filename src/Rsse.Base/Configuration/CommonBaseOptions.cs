namespace SearchEngine.Configuration;

public class CommonBaseOptions
{
    // разрешение создания бэкапа для каждой новой песни:
    public bool CreateBackupForNewSong { get; set; } = false;

    // активация функционала токенизации (настройка пока не подключена):
    public bool TokenizerIsEnable { get; set; } = false;
}
