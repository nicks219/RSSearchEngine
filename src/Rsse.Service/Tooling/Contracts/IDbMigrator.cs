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
    /// <returns>Путь к созданному файлу миграции.</returns>
    public string Create(string? fileName);

    /// <summary>
    /// Применить миграцию.
    /// </summary>
    /// <param name="fileName">Имя существующего файла миграции, при пустом аргументе используются имена из ротации.</param>
    /// <returns>Путь к существующему файлу миграции.</returns>
    public string Restore(string? fileName);

    /// <summary>
    /// Скопировать данные из MySql в Postgres.
    /// </summary>
    // todo: удалить из контракта после завершения перехода на Postgres
    public Task CopyDbFromMysqlToNpgsql();
}
