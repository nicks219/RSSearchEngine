using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SearchEngine.Data.Configuration;

namespace SearchEngine.Data.Entities;

/// <summary>
/// Представление строки таблицы бд с заметками
/// </summary>
[Table("Text")]
public class NoteEntity
{
    [Column("TextID")]
    public int NoteId { get; set; }

    [Column("Title")]
    [MaxLength(CommonDataConstants.MaxTitleLength)]
    public string? Title { get; set; }

    [Column("Song")]
    [MaxLength(CommonDataConstants.MaxTextLength)]
    public string? Text { get; set; }

    public ICollection<TagsToNotesEntity>? RelationEntityReference { get; set; }
}
