namespace SearchEngine.Domain.Dto;

/// <summary>
/// Шаблон передачи данных авторизации
/// </summary>
public record struct CredentialsDto
{
    public required string Email { get; init; }

    public required string Password { get; init; }
}
