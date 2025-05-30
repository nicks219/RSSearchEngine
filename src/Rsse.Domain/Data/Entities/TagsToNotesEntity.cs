using System.ComponentModel.DataAnnotations.Schema;

namespace SearchEngine.Data.Entities;

/// <summary>
/// Представление строки таблицы бд, связывающей заметки и теги.
/// </summary>
[Table("TagsToNotes")]
public class TagsToNotesEntity
{
    /// <summary>
    /// Идентификатор тега.
    /// </summary>
    [Column("TagId")]
    public int TagId { get; set; }

    /// <summary>
    /// Служебное поле EF для создания связи.
    /// </summary>
    public TagEntity? TagsInRelationEntity { get; set; }

    /// <summary>
    /// Идентификатор заметки.
    /// </summary>
    [Column("NoteId")]
    public int NoteId { get; set; }

    /// <summary>
    /// Служебное поле EF для создания связи.
    /// </summary>
    public NoteEntity? NoteInRelationEntity { get; set; }
}
