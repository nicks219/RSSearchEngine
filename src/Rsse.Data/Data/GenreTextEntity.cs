using System.ComponentModel.DataAnnotations.Schema;

namespace RandomSongSearchEngine.Data;

/// <summary>
/// Строка таблицы бд, связывающая песни и их жанры
/// </summary>
public class GenreTextEntity
{
    [Column("GenreID")] 
    public int GenreId { get; set; }

    public GenreEntity? GenreInGenreText { get; set; }

    [Column("TextID")] 
    public int TextId { get; set; }

    public TextEntity? TextInGenreText { get; set; }
}