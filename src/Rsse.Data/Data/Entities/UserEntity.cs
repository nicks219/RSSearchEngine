using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SearchEngine.Data.Entities;

/// <summary>
/// Представление строки таблицы бд с информацией для авторизации
/// </summary>
[Table("Users")]
public class UserEntity
{
    /// <summary>
    /// Идентификатор пользователя
    /// </summary>
    [Column("Id")]
    public int Id { get; set; }

    /// <summary>
    /// Email для авторизации
    /// </summary>
    [Column("Email")]
    [MaxLength(30)]
    public string? Email { get; set; }

    /// <summary>
    /// Пароль для авторизации
    /// </summary>
    [Column("Password")]
    [MaxLength(30)]
    public string? Password { get; set; }
}
