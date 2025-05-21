namespace SearchEngine.Domain.Configuration;

/// <summary>
/// Общие настройки сервиса.
/// </summary>
public class CommonBaseOptions
{
    /// <summary>
    /// Разрешение на создания бэкапа для каждой новой песни.
    /// </summary>
    public bool CreateBackupForNewSong { get; set; }

    /// <summary>
    /// Разрешение активации функционала токенизации.
    /// </summary>
    public bool TokenizerIsEnable { get; set; }
}
