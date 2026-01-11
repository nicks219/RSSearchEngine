using System.Collections.Generic;
using SimpleEngine.Dto.Common;
using SimpleEngine.Dto.Inverted;

namespace SimpleEngine.Indexes;

public sealed class CommonIndices
{
    public readonly List<CommonIndex> Indices = [];

    private readonly IndexPoint.DictionaryStorageType _searchType;

    private CommonIndex _currentIndex;

    public CommonIndices(IndexPoint.DictionaryStorageType searchType)
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

    private CommonIndex CreateIndex()
    {
        var current = new CommonIndex(_searchType);
        Indices.Add(current);
        return current;
    }
}
