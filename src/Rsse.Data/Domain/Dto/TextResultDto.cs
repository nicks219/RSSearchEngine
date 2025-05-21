namespace SearchEngine.Domain.Dto;

/// <summary>
/// Контейнер ответа с текстовой нагрузкой заметки.
/// </summary>
public record TextResultDto
{
    public required string Text { get; init; }
    public required string Title { get; init; }
}
