namespace SearchEngine.Configuration;

public class CommonBaseOptions
{
    // разрешение создания бэкапа для каждой новой песни:
    public bool CreateBackupForNewSong { get; init; }

    // активация функционала токенизации:
    public bool TokenizerIsEnable { get; set; }
}
