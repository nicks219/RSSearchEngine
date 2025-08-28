using System.Collections.Generic;
using RsseEngine.Dto;
using RsseEngine.Dto.Offsets;

namespace RsseEngine.Indexes;

public sealed class InvertedTfIdfIndexPartitions
{
    public readonly List<InvertedTfIdfIndex> Indices = new();

    private readonly Dictionary<Token, long> _tokenCounts = new();

    private readonly DocumentDataPoint.DocumentDataPointSearchType _searchType;

    private InvertedTfIdfIndex _currentIndex;

    private ulong _documentsCount;

    private ulong _documentsLength;

    public InvertedTfIdfIndexPartitions(
        DocumentDataPoint.DocumentDataPointSearchType searchType)
    {
        _searchType = searchType;

        _currentIndex = CreateIndex();
    }

    public double AverageDocumentSize
    {
        get
        {
            if (_documentsCount == 0UL)
            {
                return 0D;
            }

            return (double)_documentsLength / _documentsCount;
        }
    }

    public void AddOrUpdateVector(DocumentId documentId, TokenVector tokenVector)
    {
        RemoveVector(documentId);

        while (!_currentIndex.AddOrUpdateVector(documentId, tokenVector))
        {
            _currentIndex = CreateIndex();

            _documentsCount++;
            _documentsLength += (ulong)tokenVector.Count;
        }
    }

    public void RemoveVector(DocumentId documentId)
    {
        foreach (var invertedIndex in Indices)
        {
            if (invertedIndex.RemoveVector(documentId, out var documentSize))
            {
                _documentsCount--;
                _documentsLength -= (ulong)documentSize;
                break;
            }
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

        _tokenCounts.Clear();

        _documentsCount = 0UL;
        _documentsLength = 0UL;
    }

    private InvertedTfIdfIndex CreateIndex()
    {
        var current = new InvertedTfIdfIndex(_searchType);
        Indices.Add(current);
        return current;
    }
}
