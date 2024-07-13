namespace SearchEngine.Tools.MigrationAssistant;

/// <summary>
/// Контракт функционала работы с миграциями бд
/// </summary>
public interface IDbMigrator
{
    /// <summary>
    /// Создать миграцию
    /// </summary>
    /// <param name="fileName">имя создаваемого файла миграции, при пустом аргументе используются имена из ротации</param>
    /// <returns>путь к созданному файлу миграции</returns>
    public string Create(string? fileName);

    /// <summary>
    /// Применить миграцию
    /// </summary>
    /// <param name="fileName">имя существующего файла миграции, при пустом аргументе используются имена из ротации</param>
    /// <returns>путь к существующему файлу миграции</returns>
    public string Restore(string? fileName);
}
