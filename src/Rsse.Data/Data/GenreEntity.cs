using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SearchEngine.Data;

/// <summary>
/// Строка таблицы бд с тегами заметок
/// </summary>
public class GenreEntity
{
    [Column("GenreID")]
    public int TagId { get; set; }

    [MaxLength(30)]
    [Column("Genre")]
    public string? Tag { get; set; }

    public ICollection<GenreTextEntity>? GenreTextInGenre { get; set; }
}
