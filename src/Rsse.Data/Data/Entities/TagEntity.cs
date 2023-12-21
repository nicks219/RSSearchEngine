using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SearchEngine.Data.Entities;

/// <summary>
/// Представление строки таблицы бд с тегами заметок
/// </summary>
[Table("Genre")]
public class TagEntity
{
    [Column("GenreID")]
    public int TagId { get; set; }

    [MaxLength(30)]
    [Column("Genre")]
    public string? Tag { get; set; }

    public ICollection<TagsToNotesEntity>? RelationEntityReference { get; set; }
}
