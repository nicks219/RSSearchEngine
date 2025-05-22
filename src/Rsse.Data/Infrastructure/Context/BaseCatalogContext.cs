using Microsoft.EntityFrameworkCore;
using SearchEngine.Data.Entities;

// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace SearchEngine.Infrastructure.Context;

/// <summary>
/// Базовый класс для контекста источников данных.
/// </summary>
/// <param name="options">настройки контекста</param>
public abstract class BaseCatalogContext(DbContextOptions options) : DbContext(options)
{
    /// <summary>
    /// Представление таблицы с пользователемя приложения.
    /// </summary>
    public DbSet<UserEntity> Users { get; set; }

    /// <summary>
    /// Представление таблицы с заметками.
    /// </summary>
    public DbSet<NoteEntity> Notes { get; set; }

    /// <summary>
    /// Представление таблицы с тегами.
    /// </summary>
    public DbSet<TagEntity> Tags { get; set; }

    /// <summary>
    /// Представление таблицы для связи заметок и тегов.
    /// </summary>
    public DbSet<TagsToNotesEntity> TagsToNotesRelation { get; set; }

    /// <inheritdoc/>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserEntity>().HasKey(x => x.Id);

        modelBuilder.Entity<NoteEntity>().HasKey(noteEntity => noteEntity.NoteId);
        modelBuilder.Entity<NoteEntity>().HasAlternateKey(noteEntity => noteEntity.Title);

        modelBuilder.Entity<TagEntity>().HasKey(tagsEntity => tagsEntity.TagId);
        modelBuilder.Entity<TagEntity>().HasAlternateKey(tagsEntity => tagsEntity.Tag);

        modelBuilder.Entity<TagsToNotesEntity>()
            .HasKey(relationEntity => new { GenreID = relationEntity.TagId, TextID = relationEntity.NoteId });

        modelBuilder.Entity<TagsToNotesEntity>()
            .HasOne(relationEntity => relationEntity.TagsInRelationEntity)
            .WithMany(tagsEntity => tagsEntity.RelationEntityReference)
            .HasForeignKey(relationEntity => relationEntity.TagId);

        modelBuilder.Entity<TagsToNotesEntity>()
            .HasOne(relationEntity => relationEntity.NoteInRelationEntity)
            .WithMany(noteEntity => noteEntity.RelationEntityReference)
            .HasForeignKey(relationEntity => relationEntity.NoteId);

        // modelBuilder.Entity<UserEntity>().Property(x => x.Id).ValueGeneratedOnAdd();
        // modelBuilder.Entity<NoteEntity>().Property(noteEntity => noteEntity.NoteId).ValueGeneratedOnAdd();
        // modelBuilder.Entity<TagEntity>().Property(tagsEntity => tagsEntity.TagId).ValueGeneratedOnAdd();
    }
}
