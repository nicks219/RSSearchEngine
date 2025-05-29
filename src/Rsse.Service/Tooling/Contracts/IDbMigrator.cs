using System.Threading;
using System.Threading.Tasks;

namespace SearchEngine.Tooling.Contracts;

/// <summary>
/// Контракт функционала работы с миграциями бд.
/// </summary>
public interface IDbMigrator
{
    /// <summary>
    /// Создать миграцию.
    /// </summary>
    /// <param name="fileName">Имя создаваемого файла миграции, при пустом аргументе используются имена из ротации.</param>
    /// <param name="stoppingToken">Токен отмены.</param>
    /// <returns>Путь к созданному файлу миграции.</returns>
    public Task<string> Create(string? fileName, CancellationToken stoppingToken);

    /// <summary>
    /// Применить миграцию.
    /// </summary>
    /// <param name="fileName">Имя существующего файла миграции, при пустом аргументе используются имена из ротации.</param>
    /// <param name="stoppingToken">Токен отмены.</param>
    /// <returns>Путь к существующему файлу миграции.</returns>
    public Task<string> Restore(string? fileName, CancellationToken stoppingToken);

    /// <summary>
    /// Скопировать данные из MySql в Postgres.
    /// </summary>
    /// <param name="stoppingToken">Токен отмены.</param>
    // todo: удалить из контракта после завершения перехода на Postgres
    public Task CopyDbFromMysqlToNpgsql(CancellationToken stoppingToken);
}
