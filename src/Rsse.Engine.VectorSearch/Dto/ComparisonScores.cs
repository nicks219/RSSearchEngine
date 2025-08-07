using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using RsseEngine.Contracts;

namespace RsseEngine.Dto;

/// <summary>
/// Метрики для документов.
/// </summary>
/// <param name="comparisonScores">Словарь метрик документов.</param>
public readonly ref struct ComparisonScores(Dictionary<DocumentId, int> comparisonScores)
    : IEquatable<ComparisonScores>
{
    // Токенизированное слово.
    private readonly Dictionary<DocumentId, int> _comparisonScores = comparisonScores;

    /// <summary>
    /// Получить словарь метрик документов.
    /// </summary>
    // ReSharper disable once ConvertToAutoPropertyWhenPossible
    public Dictionary<DocumentId, int> Dictionary => _comparisonScores;

    public int Count => _comparisonScores.Count;

    public Dictionary<DocumentId, int>.Enumerator GetEnumerator()
    {
        return _comparisonScores.GetEnumerator();
    }

    public bool Equals(ComparisonScores other) => _comparisonScores.Equals(other._comparisonScores);

    public override bool Equals(object? obj) => obj is Token other && Equals(other);

    public override int GetHashCode() => _comparisonScores.GetHashCode();

    public static bool operator ==(ComparisonScores left, ComparisonScores right) => left.Equals(right);

    public static bool operator !=(ComparisonScores left, ComparisonScores right) => !(left == right);

    public void AddAll<TDocumentIdCollection>(TDocumentIdCollection documentIds)
        where TDocumentIdCollection : struct, IDocumentIdCollection<TDocumentIdCollection>
    {
        foreach (var documentId in documentIds)
        {
            Add(documentId);
        }
    }

    private void Add(DocumentId documentId)
    {
        ref var scoreRef = ref CollectionsMarshal.GetValueRefOrAddDefault(_comparisonScores,
            documentId, out _);

        ++scoreRef;
    }

    public bool IncrementIfExists(DocumentId documentId, out int score)
    {
        ref var scoreRef = ref CollectionsMarshal.GetValueRefOrNullRef(_comparisonScores, documentId);

        if (Unsafe.IsNullRef(ref scoreRef))
        {
            score = 0;
            return false;
        }

        ++scoreRef;
        score = scoreRef;
        return true;
    }

    public void Remove(DocumentId documentId)
    {
        _comparisonScores.Remove(documentId);
    }

    public bool Remove(DocumentId documentId, out int score)
    {
        return _comparisonScores.Remove(documentId, out score);
    }
}
