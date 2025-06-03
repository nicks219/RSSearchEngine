namespace SearchEngine.Data.Dto;

/// <summary>
/// Контейнер запроса данных авторизации.
/// </summary>
public record struct CredentialsRequestDto
{
    /// <summary>
    /// Электронная почта либо логин.
    /// </summary>
    public required string Email { get; init; }

    /// <summary>
    /// Пароль.
    /// </summary>
    public required string Password { get; init; }
}
