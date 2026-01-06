using System;
using System.Collections.Generic;
using RsseEngine.Iterators;

namespace RsseEngine.Dto.Common;

/// <summary>
/// Сортированный вектор с уникальными идентификаторами документов, используется в GIN.
/// </summary>
/// <param name="list">Список идентификаторов документов.</param>
public readonly struct InternalDocumentIds(List<InternalDocumentId> list) : IEquatable<InternalDocumentIds>
{
    // Коллекция уникальных идентификаторов заметок.
    private readonly List<InternalDocumentId> _list = list;

    public int Count => _list.Count;

    /// <summary>
    /// Получить перечислитель для вектора.
    /// </summary>
    /// <returns>Перечислитель для вектора.</returns>
    public List<InternalDocumentId>.Enumerator GetEnumerator() => _list.GetEnumerator();

    /// <summary>
    /// Получить перечислитель для вектора идентификаторов.
    /// </summary>
    /// <returns>Перечислитель.</returns>
    public InternalDocumentListEnumerator CreateDocumentListEnumerator()
    {
        return new InternalDocumentListEnumerator(_list);
    }

    public bool Equals(InternalDocumentIds other) => _list.Equals(other._list);

    public override bool Equals(object? obj) => obj is InternalDocumentIds other && Equals(other);

    public override int GetHashCode() => _list.GetHashCode();

    public static bool operator ==(InternalDocumentIds left, InternalDocumentIds right) => left.Equals(right);

    public static bool operator !=(InternalDocumentIds left, InternalDocumentIds right) => !(left == right);

    /// <summary>
    /// Добавить идентификатор документа в вектор с сохранением сортировки.
    /// </summary>
    /// <param name="documentId">Идентификатор документа.</param>
    public void Add(InternalDocumentId documentId)
    {
        if (_list.Count > 0 && documentId <= _list[_list.Count - 1])
        {
            throw new InvalidOperationException();
        }

        _list.Add(documentId);
    }
}
