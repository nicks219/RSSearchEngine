namespace Rsse.Domain.Data.Dto;

/// <summary>
/// Контейнер с ответом, представляющим тег с признаком соответствия обрабатываемой заметке.
/// </summary>
/// <param name="Tag">Именование тега.</param>
/// <param name="TagId">Идентификатор тега.</param>
/// <param name="RelationEntityReferenceCount">Количество заметок по тегу.</param>
/// <param name="IsChecked">Признак соответствия тега.</param>
public record TagMarkedResultDto(string Tag, int TagId, int RelationEntityReferenceCount, bool IsChecked)
    : TagResultDto(Tag, TagId, RelationEntityReferenceCount);
