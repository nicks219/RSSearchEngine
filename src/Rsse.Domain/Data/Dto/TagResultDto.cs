namespace SearchEngine.Data.Dto;

/// <summary>
/// Контейнер с ответом, представляющим тег.
/// </summary>
/// <param name="Tag">Именование тега.</param>
/// <param name="TagId">Идентификатор тега.</param>
/// <param name="RelationEntityReferenceCount">Количество заметок по тегу.</param>
public record TagResultDto(string Tag, int TagId, int RelationEntityReferenceCount)
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
