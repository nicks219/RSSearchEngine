using System.Collections.Generic;
using RsseEngine.Dto.Common;
using RsseEngine.Dto.Inverted;

namespace RsseEngine.Indexes;

public sealed class InvertedIndexes
{
    public readonly List<InvertedIndex> Indices = [];

    private readonly IndexPoint.DictionaryStorageType _searchType;

    private InvertedIndex _currentIndex;

    public InvertedIndexes(IndexPoint.DictionaryStorageType searchType)
    {
        _searchType = searchType;

        _currentIndex = CreateIndex();
    }

    public void AddOrUpdateVector(DocumentId documentId, TokenVector tokenVector)
    {
        foreach (var invertedIndex in Indices)
        {
            invertedIndex.RemoveVector(documentId);
        }

        while (!_currentIndex.AddOrUpdateVector(documentId, tokenVector))
        {
            _currentIndex = CreateIndex();
        }
    }

    public void RemoveVector(DocumentId documentId)
    {
        foreach (var invertedIndex in Indices)
        {
            invertedIndex.RemoveVector(documentId);
        }
    }

    public void Clear()
    {
        foreach (var invertedIndex in Indices)
        {
            invertedIndex.Clear();
        }

        Indices.Clear();

        _currentIndex = CreateIndex();
    }

    private InvertedIndex CreateIndex()
    {
        var current = new InvertedIndex(_searchType);
        Indices.Add(current);
        return current;
    }
}
