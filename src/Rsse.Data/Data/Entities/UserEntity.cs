using System.ComponentModel.DataAnnotations;

namespace SearchEngine.Data.Entities;

/// <summary>
/// Представление строки таблицы бд с информацией для авторизации
/// </summary>
public class UserEntity
{
    /// <summary>
    /// Идентификатор пользователя
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Email для авторизации
    /// </summary>
    [MaxLength(30)]
    public string? Email { get; set; }

    /// <summary>
    /// Пароль для авторизации
    /// </summary>
    [MaxLength(30)]
    public string? Password { get; set; }
}
