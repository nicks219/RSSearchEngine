using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using RsseEngine.Dto.Offsets;
using RsseEngine.Indexes;

namespace RsseEngine.Iterators;

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
    /// Оптимизация хранения позиций описана в <see cref="InvertedOffsetIndex.AppendTokenVector"/>
    /// </summary>
    /// <param name="position"></param>
    /// <returns></returns>
    public bool FindNextPosition(ref int position)
    {
        int currentIndex = _enumerator.NextIndex - 1;

        OffsetInfo offsetInfo = documentIdsWithOffsets.OffsetInfos[currentIndex];

        int size = offsetInfo.Size;

        if (size > 0)
        {
            Span<int> offsets = CollectionsMarshal.AsSpan(documentIdsWithOffsets.Offsets)
                .Slice(offsetInfo.OffsetIndex, size);

            /*
            foreach (var offset in offsets)
            {
                if (offset > position)
                {
                    position = offset;
                    return true;
                }
            }
            return false;
            /*/
            int offset = offsets.BinarySearch(position + 1);

            if (offset < 0)
            {
                offset = ~offset;
                if (offset == offsets.Length)
                {
                    return false;
                }
            }

            position = offsets[offset];
            return true;
            //*/
        }
        else
        {
            int offset = -size;

            if (offset > position)
            {
                position = offset;
                return true;
            }

            offset = offsetInfo.OffsetIndex;

            if (offset < 0)
            {
                offset = -offset;

                if (offset > position)
                {
                    position = offset;
                    return true;
                }
            }

            return false;
        }
    }

    public override string ToString()
    {
        return $"NextIndex {NextIndex} Current {Current}";
    }
}
