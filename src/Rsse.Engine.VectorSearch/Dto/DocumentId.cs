using RsseEngine.Contracts;

namespace RsseEngine.Dto;

/// <summary>
/// Идентификатор заметки.
/// В данной версии соответствует идентификатору из базы данных.
/// </summary>
/// <param name="documentId"></param>
public readonly struct DocumentId(int documentId) : IDocumentId<DocumentId>
{
    // Идентификатор заметки.
    private readonly int _documentId = documentId;

    /// <summary>
    /// Получить значение идентификатора заметки.
    /// </summary>
    // ReSharper disable once ConvertToAutoPropertyWhenPossible
    public int Value => _documentId;

    public bool Equals(DocumentId other) => _documentId.Equals(other._documentId);

    public override bool Equals(object? obj) => obj is DocumentId other && Equals(other);

    public override int GetHashCode() => _documentId;

    public static bool operator ==(DocumentId left, DocumentId right) => left.Equals(right);

    public static bool operator !=(DocumentId left, DocumentId right) => !(left == right);

    public static bool operator >(DocumentId left, DocumentId right) => left._documentId > right._documentId;

    public static bool operator <(DocumentId left, DocumentId right) => left._documentId < right._documentId;

    public static bool operator >=(DocumentId left, DocumentId right) => left._documentId >= right._documentId;

    public static bool operator <=(DocumentId left, DocumentId right) => left._documentId <= right._documentId;

    public int CompareTo(DocumentId other)
    {
        return _documentId.CompareTo(other._documentId);
    }

    public override string ToString()
    {
        return _documentId.ToString();
    }
}
