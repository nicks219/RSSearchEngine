namespace SearchEngine.Domain.Dto;

/// <summary>
/// Контейнер для обновления данных авторизации
/// </summary>
public record struct UpdateCredosRequestDto
{
    public required CredentialsRequestDto OldCredos { get; init; }
    public required CredentialsRequestDto NewCredos { get; init; }
}
