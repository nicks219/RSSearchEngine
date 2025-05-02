namespace SearchEngine.Domain.Dto;

/// <summary>
/// Контейнер для обновления данных авторизации
/// </summary>
public record struct UpdateCredosDto
{
    public required CredentialsDto OldCredos { get; init; }
    public required CredentialsDto NewCredos { get; init; }
}
