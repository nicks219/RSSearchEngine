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
    internal readonly HashSet<DocId> Vector = vector;

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
    public HashSet<DocId>.Enumerator GetEnumerator() => Vector.GetEnumerator();

    public bool Equals(DocIdVector other) => Vector.Equals(other.Vector);

    public override bool Equals(object? obj) => obj is DocIdVector other && Equals(other);

    public override int GetHashCode() => Vector.GetHashCode();

    public static bool operator ==(DocIdVector left, DocIdVector right) => left.Equals(right);

    public static bool operator !=(DocIdVector left, DocIdVector right) => !(left == right);

    public bool Contains(DocId docId) => Vector.Contains(docId);

    /// <summary>
    /// Получить копию подлежащего сета идентификаторов в виде вектора.
    /// </summary>
    /// <returns>Копия вектора.</returns>
    internal DocIdVector GetCopyInternal() => new (Vector.ToHashSet());

    /// <summary>
    /// Перейти к изменению состояния вектора.
    /// </summary>
    /// <returns>Билдер.</returns>
    public DocIdVectorBuilder ToBuilder() => new(Vector);
}

