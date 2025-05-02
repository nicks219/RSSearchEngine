using Microsoft.EntityFrameworkCore;

namespace SearchEngine.Infrastructure.Context;

/// <summary>
/// Маркер для контекста Postgres.
/// </summary>
/// <param name="option">конфигурация</param>
public sealed class NpgsqlCatalogContext(DbContextOptions<NpgsqlCatalogContext> option) : BaseCatalogContext(option);
