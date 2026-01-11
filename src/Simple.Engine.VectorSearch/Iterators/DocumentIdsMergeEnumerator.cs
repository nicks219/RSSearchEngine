using System.Collections;
using System.Collections.Generic;
using SimpleEngine.Dto.Common;
using SimpleEngine.Dto.Offsets;

namespace SimpleEngine.Iterators;

public struct DocumentIdsMergeEnumerator(
    InternalDocumentIdsWithOffsets internalDocumentIdsWithOffsets,
    DocumentIdsEnumerator enumerator)
    : IEnumerator<InternalDocumentId>
{
    private DocumentIdsEnumerator _enumerator = enumerator;

    public InternalDocumentId Current => _enumerator.Current;

    object IEnumerator.Current => Current;

    public List<InternalDocumentId> InternalDocumentIds => _enumerator.InternalDocumentIds;

    private int NextIndex => _enumerator.NextIndex;

    public bool MoveNext()
    {
        return _enumerator.MoveNext();
    }

    public void Reset()
    {
        var enumerator = (IEnumerator<InternalDocumentId>)_enumerator;
        enumerator.Reset();
        _enumerator = (DocumentIdsEnumerator)enumerator;
    }

    public void Dispose()
    {
        _enumerator.Dispose();
    }

    public bool MoveNextBinarySearch(InternalDocumentId documentId)
    {
        return _enumerator.MoveNextBinarySearch(documentId);
    }

    /// <summary>
    /// Находит следующую позицию токена в документе
    /// </summary>
    /// <param name="position"></param>
    /// <returns></returns>
    public bool TryFindNextPosition(ref int position)
    {
        var currentIndex = _enumerator.NextIndex - 1;

        var offsetInfo = internalDocumentIdsWithOffsets.OffsetInfos[currentIndex];

        return offsetInfo.TryFindNextPosition(internalDocumentIdsWithOffsets.Offsets, ref position);
    }

    public override string ToString()
    {
        return $"NextIndex {NextIndex} Current {Current}";
    }
}
