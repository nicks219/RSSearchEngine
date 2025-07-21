using System.Collections.Generic;
using RsseEngine.Contracts;
using RsseEngine.Iterators;

namespace RsseEngine.Dto;

/// <summary>
/// Вектор с уникальными идентификаторами документов, используется в GIN.
/// </summary>
/// <param name="set">Сет идентификаторов документов.</param>
public readonly struct DocumentIdSet(HashSet<DocumentId> set) : IDocumentIdCollection<DocumentIdSet>
{
    // Коллекция уникальных идентификаторов заметок.
    private readonly HashSet<DocumentId> _set = set;

    public int Count => _set.Count;

    /// <summary>
    /// Получить перечислитель для вектора идентификаторов.
    /// </summary>
    /// <returns>Перечислитель.</returns>
    public DocumentIdCollectionEnumerator<DocumentIdSet> GetEnumerator()
    {
        return new DocumentIdCollectionEnumerator<DocumentIdSet>(_set);
    }

    public bool Equals(DocumentIdSet other) => _set.Equals(other._set);

    public override bool Equals(object? obj) => obj is DocumentIdSet other && Equals(other);

    public override int GetHashCode() => _set.GetHashCode();

    public static bool operator ==(DocumentIdSet left, DocumentIdSet right) => left.Equals(right);

    public static bool operator !=(DocumentIdSet left, DocumentIdSet right) => !(left == right);

    public bool Contains(DocumentId documentId) => _set.Contains(documentId);

    public void Add(DocumentId documentId) => _set.Add(documentId);

    public void Remove(DocumentId documentId) => _set.Remove(documentId);
}
