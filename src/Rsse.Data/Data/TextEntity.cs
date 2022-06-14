using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RandomSongSearchEngine.Data;

/// <summary>
/// Строка таблицы бд с текстами песен
/// </summary>
public class TextEntity
{
    [Column("TextID")] 
    public int TextId { get; set; }

    [MaxLength(50)] 
    public string? Title { get; set; }

    [MaxLength(4000)] 
    public string? Song { get; set; }

    public ICollection<GenreTextEntity>? GenreTextInText { get; set; }
}