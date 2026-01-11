using System;
using RD.RsseEngine.Contracts;

namespace RD.RsseEngine.Dto;

/// <summary>
/// Внутренний идентификатор заметки.
/// </summary>
public readonly struct InternalDocumentId : IDocumentId<InternalDocumentId>
{
    // Идентификатор заметки.
    private readonly ushort _documentId;

    /// <summary>
    /// Внутренний идентификатор заметки.
    /// </summary>
    /// <param name="documentId">Внутренний идентификатор заметки.</param>
    public InternalDocumentId(int documentId)
    {
        if (documentId < 0 || documentId > ushort.MaxValue)
        {
            throw new ArgumentOutOfRangeException(nameof(documentId), documentId,
                $"Внутренний идентификатор документа должен быть в диапазоне от 0 до {ushort.MaxValue} включительно.");
        }

        _documentId = (ushort)documentId;
    }

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

    public static bool operator >(InternalDocumentId left, InternalDocumentId right) => left._documentId > right._documentId;

    public static bool operator <(InternalDocumentId left, InternalDocumentId right) => left._documentId < right._documentId;

    public static bool operator >=(InternalDocumentId left, InternalDocumentId right) => left._documentId >= right._documentId;

    public static bool operator <=(InternalDocumentId left, InternalDocumentId right) => left._documentId <= right._documentId;

    public int CompareTo(InternalDocumentId other)
    {
        return _documentId.CompareTo(other._documentId);
    }

    public override string ToString()
    {
        return _documentId.ToString();
    }
}
