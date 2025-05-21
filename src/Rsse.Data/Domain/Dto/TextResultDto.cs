namespace SearchEngine.Domain.Dto;

/// <summary>
/// Контейнер ответа с текстовой нагрузкой заметки.
/// </summary>
public record TextResultDto
{
    /// <summary/> Текст заметки.
    public required string Text { get; init; }

    /// <summary/> Название заметки.
    public required string Title { get; init; }
}
