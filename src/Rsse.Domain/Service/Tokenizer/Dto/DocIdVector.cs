using System;
using System.Collections.Generic;
using System.Linq;

namespace SearchEngine.Service.Tokenizer.Dto;

/// <summary>
/// Вектор с уникальными идентификаторами документов, используется в GIN.
/// </summary>
/// <param name="vector">Сет идентификаторов документов.</param>
public readonly struct DocIdVector(HashSet<DocId> vector) : IEquatable<DocIdVector>
{
    // Коллекция уникальных идентификаторов заметок.
    private readonly HashSet<DocId> _vector = vector;

    /// <summary>
    /// Вызов конструктора без параметров инициализирует вектор пустым токеном.
    /// </summary>
    public DocIdVector() : this([])
    {
    }

    /// <summary>
    /// Получить перечислитель для вектора идентификаторов.
    /// </summary>
    /// <returns>Перечислитель.</returns>
    public HashSet<DocId>.Enumerator GetEnumerator() => _vector.GetEnumerator();

    public bool Equals(DocIdVector other) => _vector.Equals(other._vector);

    public override bool Equals(object? obj) => obj is DocIdVector other && Equals(other);

    public override int GetHashCode() => _vector.GetHashCode();

    public static bool operator ==(DocIdVector left, DocIdVector right) => left.Equals(right);

    public static bool operator !=(DocIdVector left, DocIdVector right) => !(left == right);

    public bool Contains(DocId docId) => _vector.Contains(docId);

    /// <summary>
    /// Получить копию подлежащего сета идентификаторов в виде вектора.
    /// </summary>
    /// <returns>Копия вектора.</returns>
    internal DocIdVector GetCopyInternal() => new(_vector.ToHashSet());

    // todo: убрать методы, меняющий состояние вектора.

    /// <summary>
    /// Добавить идентификатор документа к вектору.
    /// </summary>
    /// <param name="docId">Идентификатор документа.</param>
    internal void Add(DocId docId) => _vector.Add(docId);

    /// <summary>
    /// Удалить из текущего вектора все элементы второго.
    /// </summary>
    /// <param name="other">Вектор, элементы которого удаляются из первого.</param>
    internal void ExceptWith(DocIdVector other) => _vector.ExceptWith(other._vector);
}
