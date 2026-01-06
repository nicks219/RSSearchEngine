using System.Collections.Generic;
using RsseEngine.Dto.Common;

namespace RsseEngine.Indexes;

public sealed class InvertedOffsetIndexPartitions
{
    public readonly List<InvertedOffsetIndex> Indices = new();

    private InvertedOffsetIndex _currentIndex;

    public InvertedOffsetIndexPartitions()
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
