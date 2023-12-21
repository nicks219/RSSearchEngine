using System.ComponentModel.DataAnnotations.Schema;

namespace SearchEngine.Data.Entities;

/// <summary>
/// Представление строки таблицы бд, связывающей заметки и теги
/// </summary>
[Table("GenreText")]
public class TagsToNotesEntity
{
    [Column("GenreID")]
    public int TagId { get; set; }

    public TagEntity? TagsInRelationEntity { get; set; }

    [Column("TextID")]
    public int NoteId { get; set; }

    public NoteEntity? NoteInRelationEntity { get; set; }
}
