using RsseEngine.Contracts;

namespace RsseEngine.Dto.Offsets;

/// <summary>
/// Внутренний идентификатор заметки.
/// </summary>
/// <param name="documentId"></param>
public readonly struct InternalDocumentId(int documentId) : IDocumentId<InternalDocumentId>
{
    // Идентификатор заметки.
    private readonly int _documentId = documentId;

    /// <summary>
    /// Получить значение идентификатора заметки.
    /// </summary>
    // ReSharper disable once ConvertToAutoPropertyWhenPossible
    public int Value => _documentId;

    public bool Equals(InternalDocumentId other) => _documentId.Equals(other._documentId);

    public override bool Equals(object? obj) => obj is InternalDocumentId other && Equals(other);

    public override int GetHashCode() => _documentId;

    public static bool operator ==(InternalDocumentId left, InternalDocumentId right) => left.Equals(right);

    public static bool operator !=(InternalDocumentId left, InternalDocumentId right) => !(left == right);

    public int CompareTo(InternalDocumentId other)
    {
        return _documentId.CompareTo(other._documentId);
    }

    public override string ToString()
    {
        return _documentId.ToString();
    }
}
