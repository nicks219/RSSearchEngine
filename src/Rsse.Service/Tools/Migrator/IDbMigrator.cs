namespace SearchEngine.Tools.Migrator;

/// <summary>
/// Контракт функционала работы с миграциями бд
/// </summary>
public interface IDbMigrator
{
    /// <summary>
    /// Создать миграцию
    /// </summary>
    /// <param name="fileName">имя создаваемого файла миграции</param>
    /// <returns>путь к созданному файлу миграции</returns>
    public string Create(string? fileName);

    /// <summary>
    /// Применить миграцию
    /// </summary>
    /// <param name="fileName">имя существующего файла миграции</param>
    /// <returns>путь к существующему файлу миграции</returns>
    public string Restore(string? fileName);
}
