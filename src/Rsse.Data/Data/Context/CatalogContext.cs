using System;
using Microsoft.EntityFrameworkCore;
using SearchEngine.Data.Entities;
using SearchEngine.Data.Repository.Scripts;

namespace SearchEngine.Data.Context;

/// <summary>
/// Контекст базы данных
/// </summary>
public sealed class CatalogContext : DbContext
{
    private readonly object _obj = new();
    private static volatile bool _init;

    /// <summary>
    /// Создать контекст работы с базой данных
    /// </summary>
    public CatalogContext(DbContextOptions<CatalogContext> option) : base(option)
    {
        if (_init)
        {
            return;
        }

        lock (_obj)
        {
            _init = true;

            var created = Database.EnsureCreated();

            switch (Database.ProviderName)
            {
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

/* аналогичный код на SQL:
CREATE TABLE [dbo].[Genre] (
    [GenreID] INT           IDENTITY (1, 1) NOT NULL,
    [Genre]   NVARCHAR (30) NOT NULL,
    PRIMARY KEY CLUSTERED ([GenreID] ASC),
    CONSTRAINT [GK-2] UNIQUE NONCLUSTERED ([Genre] ASC)
);

CREATE TABLE [dbo].[GenreText] (
    [GenreID] INT NOT NULL,
    [TextID]  INT NOT NULL,
    CONSTRAINT [NK_1] UNIQUE NONCLUSTERED ([GenreID] ASC, [TextID] ASC),
    CONSTRAINT [FK_1_Genre] FOREIGN KEY ([GenreID]) REFERENCES [dbo].[Genre] ([GenreID]),
    CONSTRAINT [FK_2_Text] FOREIGN KEY ([TextID]) REFERENCES [dbo].[Text] ([TextID]) ON DELETE CASCADE
);

CREATE TABLE [dbo].[Text] (
    [TextID] INT             IDENTITY (1, 1) NOT NULL,
    [Title]  NVARCHAR (50)   NOT NULL,
    [Song]   NVARCHAR (4000) NOT NULL,
    PRIMARY KEY CLUSTERED ([TextID] ASC),
    CONSTRAINT [NK_2] UNIQUE NONCLUSTERED ([Title] ASC)
);*/
