using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using Rsse.Domain.Data.Configuration;

namespace Rsse.Domain.Data.Entities;

/// <summary>
/// Представление строки таблицы бд с заметкой.
/// </summary>
[Table("Note")]
// свойства entity должны содержать метод set
[SuppressMessage("ReSharper", "PropertyCanBeMadeInitOnly.Global")]
public class NoteEntity
{
    /// <summary>
    /// Идентификатор заметки.
    /// </summary>
    [Column("NoteId")]
    public int NoteId { get; set; }

    /// <summary>
    /// Именование заметки.
    /// </summary>
    [Column("Title")]
    [MaxLength(CommonDataConstants.MaxTitleLength)]
    public required string Title { get; set; }

    /// <summary>
    /// Текст заметки.
    /// </summary>
    [Column("Text")]
    [MaxLength(CommonDataConstants.MaxTextLength)]
    public required string Text { get; set; }

    /// <summary>
    /// Служебное поле EF для создания связи.
    /// </summary>
    public ICollection<TagsToNotesEntity> RelationEntityReference { get; set; } = null!;
}
