using System.Collections;
using System.Collections.Generic;
using RD.RsseEngine.Dto;
using RD.RsseEngine.Dto.Offsets;

namespace RD.RsseEngine.Iterators;

public struct TokenOffsetEnumerator(DocumentIdsWithOffsets documentIdsWithOffsets,
    InternalDocumentListEnumerator enumerator)
    : IEnumerator<InternalDocumentId>
{
    private InternalDocumentListEnumerator _enumerator = enumerator;

    public InternalDocumentId Current => _enumerator.Current;

    object IEnumerator.Current => Current;

    public List<InternalDocumentId> List => _enumerator.List;

    private int NextIndex => _enumerator.NextIndex;

    public bool MoveNext()
    {
        return _enumerator.MoveNext();
    }

    public void Reset()
    {
        var enumerator = (IEnumerator<InternalDocumentId>)_enumerator;
        enumerator.Reset();
        _enumerator = (InternalDocumentListEnumerator)enumerator;
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

        var offsetInfo = documentIdsWithOffsets.OffsetInfos[currentIndex];

        return offsetInfo.TryFindNextPosition(documentIdsWithOffsets.Offsets, ref position);
    }

    public override string ToString()
    {
        return $"NextIndex {NextIndex} Current {Current}";
    }
}
