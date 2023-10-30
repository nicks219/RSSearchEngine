using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SearchEngine.Data.Configuration;

namespace SearchEngine.Data;

/// <summary>
/// Строка таблицы бд с текстами песен
/// </summary>
public class TextEntity
{
    [Column("TextID")]
    public int TextId { get; set; }

    [MaxLength(CommonDataConstants.MaxTitleLength)]
    public string? Title { get; set; }

    [MaxLength(CommonDataConstants.MaxTextLenght)]
    public string? Song { get; set; }

    public ICollection<GenreTextEntity>? GenreTextInText { get; set; }
}
