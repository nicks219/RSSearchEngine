namespace SearchEngine.Domain.Dto;

/// <summary>
/// Контейнер запроса с текстовой нагрузкой заметки.
/// </summary>
public record TextRequestDto
{
    public required string? Text { get; init; }
    public required string? Title { get; init; }
}
