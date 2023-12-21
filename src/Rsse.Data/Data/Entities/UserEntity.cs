using System.ComponentModel.DataAnnotations;

namespace SearchEngine.Data.Entities;

/// <summary>
/// Представление строки таблицы бд с информацией для авторизации
/// </summary>
public class UserEntity
{
    public int Id { get; set; }

    [MaxLength(30)]
    public string? Email { get; set; }

    [MaxLength(30)]
    public string? Password { get; set; }
}
