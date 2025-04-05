using Microsoft.EntityFrameworkCore;
using SearchEngine.Data.Entities;

namespace SearchEngine.Data.Context;

/// <summary>
/// Базовый класс для контекста источников данных.
/// </summary>
/// <param name="options">настройки контекста</param>
public abstract class BaseCatalogContext(DbContextOptions options) : DbContext(options)
{
    /// <summary>
    /// Контекст для пользователей приложения
    /// </summary>
    public virtual DbSet<UserEntity>? Users { get; set; }

    /// <summary>
    /// Контекст для текстов заметок
    /// </summary>
    public virtual DbSet<NoteEntity>? Notes { get; set; }

    /// <summary>
    /// Контекст для тегов заметок
    /// </summary>
    public virtual DbSet<TagEntity>? Tags { get; set; }

    /// <summary>
    /// Контекст для связыви заметок и тегов
    /// </summary>
    public virtual DbSet<TagsToNotesEntity>? TagsToNotesRelation { get; set; }
}
