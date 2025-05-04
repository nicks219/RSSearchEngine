namespace SearchEngine.Domain.Dto;

/// <summary>
/// Контейнер с заметкой
/// </summary>
public record TextResult
{
    public required string Text { get; init; }
    public required string Title { get; init; }
}
