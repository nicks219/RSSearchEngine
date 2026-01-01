using System.Collections.Generic;
using RsseEngine.Dto;
using RsseEngine.Dto.Offsets;

namespace RsseEngine.Indexes;

public sealed class InvertedIndexPartitions
{
    public readonly List<InvertedIndex> Indices = new();

    private readonly DocumentDataPoint.DocumentDataPointSearchType _searchType;

    private InvertedIndex _currentIndex;

    public InvertedIndexPartitions(
        DocumentDataPoint.DocumentDataPointSearchType searchType)
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
