namespace SearchEngine.Domain.Configuration;

/// <summary>
/// Общие настройки для сервиса
/// </summary>
public class CommonBaseOptions
{
    /// <summary>
    /// Разрешение на создания бэкапа для каждой новой песни
    /// </summary>
    public bool CreateBackupForNewSong { get; set; }

    /// <summary>
    /// Активация функционала токенизации
    /// </summary>
    public bool TokenizerIsEnable { get; set; }
}
