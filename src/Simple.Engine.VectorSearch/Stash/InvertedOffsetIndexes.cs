using System.Collections.Generic;
using SimpleEngine.Dto.Common;

namespace SimpleEngine.Indexes;

public sealed class InvertedOffsetIndexes2
{
    public readonly List<InvertedOffsetIndex> Indices = [];

    private InvertedOffsetIndex _currentIndex;

    public InvertedOffsetIndexes()
    {
        _currentIndex = CreateIndex();
    }

    public void AddOrUpdateVector(DocumentId documentId, TokenVector tokenVector)
    {
        foreach (var invertedOffsetIndex in Indices)
        {
            invertedOffsetIndex.RemoveVector(documentId);
        }

        while (!_currentIndex.AddOrUpdateVector(documentId, tokenVector))
        {
            _currentIndex = CreateIndex();
        }
    }

    public void RemoveVector(DocumentId documentId)
    {
        foreach (var invertedOffsetIndex in Indices)
        {
            invertedOffsetIndex.RemoveVector(documentId);
        }
    }

    public void Clear()
    {
        foreach (var invertedOffsetIndex in Indices)
        {
            invertedOffsetIndex.Clear();
        }

        Indices.Clear();

        _currentIndex = CreateIndex();
    }

    private InvertedOffsetIndex CreateIndex()
    {
        var current = new InvertedOffsetIndex();
        Indices.Add(current);
        return current;
    }
}
