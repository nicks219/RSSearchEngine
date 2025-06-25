using System;
using System.Collections.Generic;
using System.Linq;

namespace Rsse.Search.Dto;

/// <summary>
/// Сортированый вектор с уникальными идентификаторами документов, используется в GIN.
/// </summary>
/// <param name="list">Сет идентификаторов документов.</param>
public readonly struct DocumentIdList(List<DocumentId> list) : IEquatable<DocumentIdList>, IDocumentIdCollection
{
    // Коллекция уникальных идентификаторов заметок.
    private readonly List<DocumentId> _list = list;

    public int Count => _list.Count;

    /// <summary>
    /// Получить перечислитель для вектора идентификаторов.
    /// </summary>
    /// <returns>Перечислитель.</returns>
    public List<DocumentId>.Enumerator GetEnumerator() => _list.GetEnumerator();

    public bool Equals(DocumentIdList other) => _list.Equals(other._list);

    public override bool Equals(object? obj) => obj is DocumentIdList other && Equals(other);

    public override int GetHashCode() => _list.GetHashCode();

    public static bool operator ==(DocumentIdList left, DocumentIdList right) => left.Equals(right);

    public static bool operator !=(DocumentIdList left, DocumentIdList right) => !(left == right);

    public bool Contains(DocumentId documentId)
    {
        return _list.BinarySearch(documentId) >= 0;
    }

    public void Add(DocumentId documentId)
    {
        if (_list.Count == 0 || _list[_list.Count - 1].Value < documentId.Value)
        {
            _list.Add(documentId);
        }
        else
        {
            int itemIndex = _list.BinarySearch(documentId);
            if (itemIndex < 0)
            {
                itemIndex = ~itemIndex; // bitwise complement - index is next larger index
                _list.Insert(itemIndex, documentId);
            }
        }
    }

    public void Remove(DocumentId documentId)
    {
        int itemIndex = _list.BinarySearch(documentId);
        if (itemIndex >= 0)
        {
            _list.RemoveAt(itemIndex);
        }
    }

    public void ExceptWith(DocumentIdList other)
    {
        foreach (DocumentId documentId in other._list)
        {
            _list.Remove(documentId);
        }
    }

    public DocumentIdList GetCopyInternal() => new(_list.ToList());
}
