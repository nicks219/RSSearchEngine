namespace SearchEngine.Infrastructure;

/// <summary>
/// Контракт функционала работы с миграциями бд
/// </summary>
public interface IDbMigrator
{
    public string Create(string? fileName);

    public string Restore(string? fileName);
}
