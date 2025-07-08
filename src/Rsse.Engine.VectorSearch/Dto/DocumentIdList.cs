using System;
using System.Collections.Generic;
using RsseEngine.Contracts;

namespace RsseEngine.Dto;

/// <summary>
/// Сортированный вектор с уникальными идентификаторами документов, используется в GIN.
/// </summary>
/// <param name="list">Список идентификаторов документов.</param>
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

    /// <summary>
    /// Добавить идентификатор документа в вектор с сохранением сортировки.
    /// </summary>
    /// <param name="documentId">Идентификатор документа.</param>
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
                // при побитовом дополнении значение будет указывать на следующий больший индекс
                // "bitwise complement - index is next larger index"
                itemIndex = ~itemIndex;
                _list.Insert(itemIndex, documentId);
            }
        }
    }

    /// <summary>
    /// Удалить идентификатор документа из вектора.
    /// </summary>
    /// <param name="documentId">Идентификатор документа.</param>
    public void Remove(DocumentId documentId)
    {
        int itemIndex = _list.BinarySearch(documentId);
        if (itemIndex >= 0)
        {
            _list.RemoveAt(itemIndex);
        }
    }

    /// <summary>
    /// Получить копию подлежащего сета идентификаторов в виде вектора за исключением существующих данных.
    /// </summary>
    /// <param name="exceptSet">Коллекция с существующими данными.</param>
    /// <returns>Копия вектора.</returns>
    public DocumentIdList CopyExcept(HashSet<DocumentId> exceptSet)
    {
        DocumentIdList expectResult = new DocumentIdList(new List<DocumentId>());

        foreach (DocumentId documentId in _list)
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
        foreach (var documentId in _list)
        {
            if (!visitor.Visit(documentId))
            {
                break;
            }
        }
    }
}
