namespace Rsse.Domain.Data.Dto;

/// <summary>
/// Контейнер с ответом, представляющим тег с признаком соответствия обрабатываемой заметке.
/// </summary>
/// <param name="Tag">Именование тега.</param>
/// <param name="TagId">Идентификатор тега.</param>
/// <param name="RelationEntityReferenceCount">Количество заметок по тегу.</param>
/// <param name="IsChecked">Признак соответствия тега.</param>
public readonly record struct TagMarkedResultDto(string Tag, int TagId, int RelationEntityReferenceCount, bool IsChecked)
{
    /// <summary>
    /// Вернуть строку с именем тега и количеством заметок по тегу.
    /// </summary>
    /// <returns>Строка с обогащенным именем.</returns>
    public string GetEnrichedName()
    {
        var enrichedName = RelationEntityReferenceCount > 0
            ? Tag + ": " + RelationEntityReferenceCount
            : Tag;

        return enrichedName;
    }
}
