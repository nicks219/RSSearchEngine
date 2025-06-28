namespace Rsse.Domain.Data.Dto;

/// <summary>
/// Контейнер запроса для обновления данных авторизации.
/// </summary>
public record struct UpdateCredosRequestDto
{
    /// <summary/> Актуальные данные авторизации.
    public required CredentialsRequestDto OldCredos { get; init; }

    /// <summary/> Данные авторизации для обновления.
    public required CredentialsRequestDto NewCredos { get; init; }
}
