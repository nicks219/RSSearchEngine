using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Rsse.Search.Dto;

namespace Rsse.Search.Indexes;

public sealed class DirectIndex
{
    private readonly ConcurrentDictionary<DocumentId, TokenLine> _directIndex = new();

    public IEnumerator<KeyValuePair<DocumentId, TokenLine>> GetEnumerator()
    {
        return _directIndex.GetEnumerator();
    }

    public int Count => _directIndex.Count;

    public TokenLine this[DocumentId documentId] => _directIndex[documentId];

    public bool TryAdd(DocumentId documentId, TokenLine tokenLine)
    {
        return _directIndex.TryAdd(documentId, tokenLine);
    }

    public bool TryUpdate(DocumentId documentId, TokenLine tokenLine, [NotNullWhen(true)] out TokenLine? oldTokenLine)
    {
        if (!_directIndex.TryGetValue(documentId, out oldTokenLine))
        {
            return false;
        }

        if (!_directIndex.TryUpdate(documentId, tokenLine, oldTokenLine))
        {
            return false;
        }

        return true;
    }

    public bool TryRemove(DocumentId documentId, [NotNullWhen(true)] out TokenLine? oldTokenLine)
    {
        return _directIndex.TryRemove(documentId, out oldTokenLine);
    }

    public void Clear()
    {
        _directIndex.Clear();
    }

    public KeyValuePair<DocumentId, TokenLine> ElementAt(int index)
    {
        return _directIndex.ElementAt(index);
    }

    public KeyValuePair<DocumentId, TokenLine> First()
    {
        return _directIndex.First();
    }

    public KeyValuePair<DocumentId, TokenLine> Last()
    {
        return _directIndex.Last();
    }
}
