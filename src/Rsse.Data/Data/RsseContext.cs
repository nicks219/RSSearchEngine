using Microsoft.EntityFrameworkCore;
using RandomSongSearchEngine.Data.Repository;

namespace RandomSongSearchEngine.Data;

/// <summary>
/// Контекст (таблицы) базы данных
/// </summary>
public sealed class RsseContext : DbContext
{
    private readonly object _obj = new();
    private static volatile bool _init;
    
    /// <summary>
    /// Конфигурируем контекст базы данных
    /// </summary>
    /// <param name="option"></param>
    public RsseContext(DbContextOptions<RsseContext> option) : base(option)
    {
        // const string relativePath = "Dump/rsse-5-4.dump";
        // var path = Path.Combine(AppContext.BaseDirectory, relativePath);
        // var sql = File.ReadAllText(path);

        // в SqlScripts удаляю индекс для GenreText таблицы
        if (_init)
        {
            return;
        }

        lock (_obj)
        {
            _init = true;
            
            var res = Database.EnsureCreated();
            
            switch (Database.ProviderName)
            {
                case "Pomelo.EntityFrameworkCore.MySql":
                    if (res) Database.ExecuteSqlRaw(MySqlScripts.CreateGenresScript);
                    break;
                
                case "Microsoft.EntityFrameworkCore.SqlServer":
                    if (res) Database.ExecuteSqlRaw(MsSqlScripts.CreateGenresScript);
                    break;
                
                default:
                    //"Microsoft.EntityFrameworkCore.InMemory" например
                    break;
            }
        }
    }

    /// <summary>
    /// Таблица бд с пользователями приложения
    /// </summary>
    public DbSet<UserEntity>? Users { get; set; }

    /// <summary>
    /// Таблица бд с текстами песен
    /// </summary>
    public DbSet<TextEntity>? Text { get; set; }

    /// <summary>
    /// Таблица бд с жанрами песен
    /// </summary>
    public DbSet<GenreEntity>? Genre { get; set; }

    /// <summary>
    /// Таблица бд, связывающая песни и их жанры
    /// </summary>
    public DbSet<GenreTextEntity>? GenreText { get; set; }

    /// <summary>
    /// Создание связей для модели "многие-ко-многим"
    /// </summary>
    /// <param name="modelBuilder"></param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // base.OnModelCreating(modelBuilder);
        
        modelBuilder.Entity<TextEntity>()
            .HasKey(k => k.TextId);
        
        //CONSTRAINT UNIQUE NONCLUSTERED
        modelBuilder.Entity<TextEntity>()
            .HasAlternateKey(k => k.Title);

        modelBuilder.Entity<GenreEntity>()
            .HasKey(k => k.GenreId);
        
        //CONSTRAINT UNIQUE NONCLUSTERED
        modelBuilder.Entity<GenreEntity>()
            .HasAlternateKey(k => k.Genre);

        modelBuilder.Entity<GenreTextEntity>()
            .HasKey(k => new {GenreID = k.GenreId, TextID = k.TextId});
        
        modelBuilder.Entity<GenreTextEntity>()
            .HasOne(g => g.GenreInGenreText)
            .WithMany(m => m!.GenreTextInGenre)
            .HasForeignKey(k => k.GenreId);
        
        modelBuilder.Entity<GenreTextEntity>()
            .HasOne(t => t.TextInGenreText)
            .WithMany(m => m!.GenreTextInText)
            .HasForeignKey(k => k.TextId);
    }
}

/*
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
);
*/