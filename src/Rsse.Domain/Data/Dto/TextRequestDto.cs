namespace SearchEngine.Data.Dto;

/// <summary>
/// Контейнер запроса с текстовой нагрузкой заметки.
/// </summary>
public record TextRequestDto
{
    /// <summary/> Текст заметки.
    public required string? Text { get; init; }

    /// <summary/> Название заметки.
    public required string? Title { get; init; }
}
