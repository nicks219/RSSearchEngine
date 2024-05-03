using System;
using Microsoft.EntityFrameworkCore;
using SearchEngine.Data.Entities;
using SearchEngine.Data.Repository.Scripts;

namespace SearchEngine.Data.Context;

/// <summary>
/// Контекст базы данных
/// </summary>
public sealed class NpgsqlCatalogContext : DbContext
{
    private readonly object _obj = new();
    private static volatile bool _init;

    /// <summary>
    /// Создать контекст работы с базой данных
    /// </summary>
    public NpgsqlCatalogContext(DbContextOptions<NpgsqlCatalogContext> option) : base(option)
    {
        if (_init)
        {
            return;
        }

        lock (_obj)
        {
            _init = true;

            var deleted = Database.EnsureDeleted();
            var created = Database.EnsureCreated();

            switch (Database.ProviderName)
            {
                case "Npgsql.EntityFrameworkCore.PostgreSQL":
                    if (created)
                    {
                        // var raws = Database.ExecuteSqlRaw(NpgsqlScript.CreateStubData);
                        // Console.WriteLine($"[ROWS AFFECTED] {raws}");
                    }
                    break;

                case "Pomelo.EntityFrameworkCore.MySql":
                    if (created)
                    {
                        var raws = Database.ExecuteSqlRaw(MySqlScript.CreateStubData);
                        Console.WriteLine($"[ROWS AFFECTED] {raws}");
                    }
                    break;

                // SQLite используется при запуске интеграционных тестов:
                case "Microsoft.EntityFrameworkCore.Sqlite":
                    // TODO можно инициализировать тестовую базу на каждый вызов контекста:
                    if (created)
                    {
                        var raws = Database.ExecuteSqlRaw(SQLiteIntegrationTestScript.CreateTestData);
                        Console.WriteLine($"[ROWS AFFECTED] {raws}");
                    }
                    break;

                case "Microsoft.EntityFrameworkCore.SqlServer":
                    if (created)
                    {
                        var raws = Database.ExecuteSqlRaw(MsSqlScript.CreateStubData);
                        Console.WriteLine($"[ROWS AFFECTED] {raws}");
                    }
                    break;
            }
        }
    }

    /// <summary>
    /// Контекст для пользователей приложения
    /// </summary>
    public DbSet<UserEntity>? Users { get; set; }

    /// <summary>
    /// Контекст для текстов заметок
    /// </summary>
    public DbSet<NoteEntity>? Notes { get; set; }

    // <summary /> удалить. сущность для слияния двух баз
    // public DbSet<TextEntity>? Texts { get; set; }
    // public DbSet<TagsToNotesEntity>? TagsToNotesRelationForText { get; set; }

    /// <summary>
    /// Контекст для тегов заметок
    /// </summary>
    public DbSet<TagEntity>? Tags { get; set; }

    /// <summary>
    /// Контекст для связыви заметок и тегов
    /// </summary>
    public DbSet<TagsToNotesEntity>? TagsToNotesRelation { get; set; }

    /// <summary>
    /// Создать связи для модели "многие-ко-многим"
    /// </summary>
    /// <param name="modelBuilder"></param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // удалить. сущность для слияния двух баз
        // modelBuilder.Entity<TextEntity>().HasKey(textEntity => textEntity.NoteId);

        modelBuilder.Entity<NoteEntity>()
            .HasKey(noteEntity => noteEntity.NoteId);

        // CONSTRAINT UNIQUE NONCLUSTERED
        modelBuilder.Entity<NoteEntity>()
            .HasAlternateKey(noteEntity => noteEntity.Title);

        modelBuilder.Entity<TagEntity>()
            .HasKey(tagsEntity => tagsEntity.TagId);

        // CONSTRAINT UNIQUE NONCLUSTERED
        modelBuilder.Entity<TagEntity>()
            .HasAlternateKey(tagsEntity => tagsEntity.Tag);

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
    }
}
