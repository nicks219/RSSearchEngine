using System;
using System.Collections.Generic;
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

    /// <summary>
    /// Получить копию подлежащего сета идентификаторов в виде вектора за исключением существующих данных.
    /// </summary>
    /// <param name="exceptSet">Коллекция с существующими данными.</param>
    /// <returns>Копия вектора.</returns>
    public DocumentIdSet CopyExcept(HashSet<DocumentId> exceptSet)
    {
        DocumentIdSet expectResult = new DocumentIdSet(new HashSet<DocumentId>());

        foreach (DocumentId documentId in _set)
        {
            if (exceptSet.Add(documentId))
            {
                expectResult.Add(documentId);
            }
        }

        return expectResult;
    }

    /// <summary>
    /// Перебирает коллекцию в цикле.
    /// </summary>
    /// <typeparam name="TVisitor">Тип посетителя цикла.</typeparam>
    /// <param name="visitor">Посетитель цикла - применяется для каждого элемента.</param>
    public void ForEach<TVisitor>(ref TVisitor visitor)
        where TVisitor : IForEachVisitor<DocumentId>, allows ref struct
    {
        foreach (var documentId in _set)
        {
            if (!visitor.Visit(documentId))
            {
                break;
            }
        }
    }
}
