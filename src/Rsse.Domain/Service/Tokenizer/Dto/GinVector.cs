using System;
using System.Collections.Generic;

namespace SearchEngine.Service.Tokenizer.Dto;

public readonly struct GinVector(HashSet<DocId> vector) : IEquatable<GinVector>
{
    private readonly HashSet<DocId> _vector = vector;

    public HashSet<DocId> Value => _vector;

    public HashSet<DocId>.Enumerator GetEnumerator() => _vector.GetEnumerator();

    public bool Equals(GinVector other) => _vector.Equals(other._vector);

    public override bool Equals(object? obj) => obj is GinVector other && Equals(other);

    public override int GetHashCode() => _vector.GetHashCode();

    public static bool operator ==(GinVector left, GinVector right) => left.Equals(right);

    public static bool operator !=(GinVector left, GinVector right) => !(left == right);

    public bool Contains(DocId docId)
    {
        return _vector.Contains(docId);
    }

    public void UnionWith(GinVector value)
    {
        _vector.UnionWith(value._vector);
    }

    public void Add(DocId docId)
    {
        _vector.Add(docId);
    }
}
