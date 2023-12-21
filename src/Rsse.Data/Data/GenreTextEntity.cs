using System.ComponentModel.DataAnnotations.Schema;

namespace SearchEngine.Data;

/// <summary>
/// Строка таблицы бд, связывающая заметки и теги
/// </summary>
public class GenreTextEntity
{
    [Column("GenreID")]
    public int TagId { get; set; }

    public GenreEntity? GenreInGenreText { get; set; }

    [Column("TextID")]
    public int NoteId { get; set; }

    public TextEntity? TextInGenreText { get; set; }
}
