using System;
using System.Collections.Generic;
using System.Linq;
using RsseEngine.Contracts;

namespace RsseEngine.Dto;

/// <summary>
/// Вектор с уникальными идентификаторами документов, используется в GIN.
/// </summary>
/// <param name="set">Сет идентификаторов документов.</param>
public readonly struct DocumentIdSet(HashSet<DocumentId> set) : IEquatable<DocumentIdSet>, IDocumentIdCollection
{
    // Коллекция уникальных идентификаторов заметок.
    private readonly HashSet<DocumentId> _set = set;

    public int Count => _set.Count;

    /// <summary>
    /// Получить перечислитель для вектора идентификаторов.
    /// </summary>
    /// <returns>Перечислитель.</returns>
    public HashSet<DocumentId>.Enumerator GetEnumerator() => _set.GetEnumerator();

    public bool Equals(DocumentIdSet other) => _set.Equals(other._set);

    public override bool Equals(object? obj) => obj is DocumentIdSet other && Equals(other);

    public override int GetHashCode() => _set.GetHashCode();

    public static bool operator ==(DocumentIdSet left, DocumentIdSet right) => left.Equals(right);

    public static bool operator !=(DocumentIdSet left, DocumentIdSet right) => !(left == right);

    public bool Contains(DocumentId documentId) => _set.Contains(documentId);

    public void Add(DocumentId documentId) => _set.Add(documentId);

    public void Remove(DocumentId documentId) => _set.Remove(documentId);

    public void ExceptWith(DocumentIdSet other) => _set.ExceptWith(other._set);

    /// <summary>
    /// Получить копию подлежащего сета идентификаторов в виде вектора.
    /// </summary>
    /// <returns>Копия вектора.</returns>
    public DocumentIdSet GetCopyInternal() => new(_set.ToHashSet());
}
