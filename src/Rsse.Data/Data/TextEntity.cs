using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SearchEngine.Data.Options;

namespace SearchEngine.Data;

/// <summary>
/// Строка таблицы бд с текстами песен
/// </summary>
public class TextEntity
{
    [Column("TextID")]
    public int TextId { get; set; }

    [MaxLength(CommonDataOptions.MaxTitleLength)]
    public string? Title { get; set; }

    [MaxLength(CommonDataOptions.MaxTextLenght)]
    public string? Song { get; set; }

    public ICollection<GenreTextEntity>? GenreTextInText { get; set; }
}
