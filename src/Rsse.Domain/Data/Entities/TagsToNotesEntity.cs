using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace Rsse.Domain.Data.Entities;

/// <summary>
/// Представление строки таблицы бд, связывающей заметки и теги.
/// </summary>
[Table("TagsToNotes")]
// свойства entity должны содержать метод set
[SuppressMessage("ReSharper", "PropertyCanBeMadeInitOnly.Global")]
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
    public TagEntity TagsInRelationEntity { get; set; } = null!;

    /// <summary>
    /// Идентификатор заметки.
    /// </summary>
    [Column("NoteId")]
    public int NoteId { get; set; }

    /// <summary>
    /// Служебное поле EF для создания связи.
    /// </summary>
    public NoteEntity NoteInRelationEntity { get; set; } = null!;
}
