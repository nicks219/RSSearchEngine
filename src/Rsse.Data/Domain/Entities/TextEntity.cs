using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SearchEngine.Domain.Configuration;

namespace SearchEngine.Domain.Entities;

// todo: MySQL WORK. DELETE
public interface INote
{
    public int NoteId { get; set; }
    public string? Title { get; set; }
    public string? Text { get; set; }
}

/// <summary/> Представление строки таблицы бд с заметками
// todo: MySQL WORK. DELETE
[Table("Text")]
public class TextEntity : INote
{
    /// <summary>
    /// Номер заметки
    /// </summary>
    [Column("TextId")]
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
    [Column("Song")]
    [MaxLength(CommonDataConstants.MaxTextLength)]
    public string? Text { get; set; }

    // public ICollection<TagsToNotesEntity>? RelationEntityReference { get; set; }
}
