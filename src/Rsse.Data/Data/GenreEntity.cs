using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RandomSongSearchEngine.Data;

/// <summary>
/// Строка таблицы бд с жанрами песен
/// </summary>
public class GenreEntity
{
    [Column("GenreID")] 
    public int GenreId { get; set; }

    [MaxLength(30)] 
    public string? Genre { get; set; }

    public ICollection<GenreTextEntity>? GenreTextInGenre { get; set; }
}