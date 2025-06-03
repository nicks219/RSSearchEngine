using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace SearchEngine.Data.Entities;

/// <summary>
/// Представление строки таблицы бд с информацией для авторизации.
/// </summary>
[Table("Users")]
// свойства entity должны содержать метод set
[SuppressMessage("ReSharper", "PropertyCanBeMadeInitOnly.Global")]
public class UserEntity
{
    /// <summary>
    /// Идентификатор пользователя.
    /// </summary>
    [Column("Id")]
    public int Id { get; set; }

    /// <summary>
    /// Email для пользователя.
    /// </summary>
    [Column("Email")]
    [MaxLength(30)]
    public required string Email { get; set; }

    /// <summary>
    /// Пароль для пользователя.
    /// </summary>
    [Column("Password")]
    [MaxLength(30)]
    public required string Password { get; set; }
}
