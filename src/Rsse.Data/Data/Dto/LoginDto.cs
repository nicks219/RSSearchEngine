using System.ComponentModel.DataAnnotations;

namespace SearchEngine.Data.Dto;

/// <summary>
/// Шаблон передачи данных авторизации
/// </summary>
public record LoginDto
{
    /// <summary>
    /// Запись с электронной почтой
    /// </summary>
    [Required(ErrorMessage = $"[{nameof(LoginDto)}] empty email")]
    public string? Email { get; }

    /// <summary>
    /// Запись с паролем
    /// </summary>
    [Required(ErrorMessage = $"[{nameof(LoginDto)}] empty password")]
    [DataType(DataType.Password)]
    public string? Password { get; }

    /// <summary>
    /// Создать шаблон передачи данных авторизации
    /// </summary>
    public LoginDto(string email, string password)
    {
        Email = email;

        Password = password;
    }
}
