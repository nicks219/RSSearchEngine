using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SearchEngine.Data.Entities;

/// <summary>
/// Представление строки таблицы бд с тегом.
/// </summary>
[Table("Tag")]
public class TagEntity
{
    /// <summary>
    /// Идентификатор тега.
    /// </summary>
    [Column("TagId")]
    public int TagId { get; set; }

    /// <summary>
    /// Именование тега.
    /// </summary>
    [MaxLength(30)]
    [Column("Tag")]
    public required string Tag { get; set; }

    /// <summary>
    /// Служебное поле EF для создания связи.
    /// </summary>
    public ICollection<TagsToNotesEntity>? RelationEntityReference { get; set; }
}
