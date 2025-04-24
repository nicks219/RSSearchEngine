using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SearchEngine.Data.Configuration;

namespace SearchEngine.Data.Entities;

/// <summary>
/// Представление строки таблицы бд с заметками
/// </summary>
[Table("Note")]
public class NoteEntity : INote
{
    /// <summary>
    /// Номер заметки
    /// </summary>
    //[Key]
    //[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("NoteId")]
    public int NoteId { get; set; }

    /// <summary>
    /// Именование заметки
    /// </summary>
    [Column("Title")]
    [MaxLength(CommonDataConstants.MaxTitleLength)]
    public string? Title { get; set; }

    /// <summary>
    /// Текст заметки
    /// </summary>
    [Column("Text")]
    [MaxLength(CommonDataConstants.MaxTextLength)]
    public string? Text { get; set; }

    /// <summary>
    /// Служебное поле EF для создания связи
    /// </summary>
    public ICollection<TagsToNotesEntity>? RelationEntityReference { get; set; }
}
