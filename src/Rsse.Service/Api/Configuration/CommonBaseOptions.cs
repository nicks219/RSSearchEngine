using RsseEngine.SearchType;

namespace Rsse.Api.Configuration;

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

    /// <summary>
    /// Алгоритм оптимизации поиска в токенайзере.
    /// </summary>
    public ExtendedSearchType ExtendedSearchType { get; set; }

    /// <summary>
    /// Алгоритм оптимизации поиска в токенайзере.
    /// </summary>
    public ReducedSearchType ReducedSearchType { get; set; }
}
