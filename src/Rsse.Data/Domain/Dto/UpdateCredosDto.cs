namespace SearchEngine.Domain.Dto;

/// <summary>
/// Контейнер для обновления данных авторизации
/// </summary>
public record struct UpdateCredosDto
{
    public required LoginDto OldCredos { get; init; }
    public required LoginDto NewCredos { get; init; }
}
