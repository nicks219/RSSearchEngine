using Microsoft.EntityFrameworkCore;

namespace SearchEngine.Infrastructure.Context;

/// <summary>
/// Маркер для контекста MySql.
/// </summary>
/// <param name="option">Настройки контекста.</param>
public sealed class MysqlCatalogContext(DbContextOptions<MysqlCatalogContext> option) : BaseCatalogContext(option);
