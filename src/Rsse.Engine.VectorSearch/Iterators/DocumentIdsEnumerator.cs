using System;
using System.Collections;
using System.Collections.Generic;
using RsseEngine.Dto.Common;

namespace RsseEngine.Iterators;

public struct DocumentIdsEnumerator(List<InternalDocumentId> internalDocumentIds) : IEnumerator<InternalDocumentId>
{
    private int _nextIndex = 0;
    private InternalDocumentId _current = default;

    public List<InternalDocumentId> InternalDocumentIds => internalDocumentIds;

    // todo: convert into auto-property
    public int NextIndex => _nextIndex;

    public void Dispose()
    {
    }

    public bool MoveNextBinarySearch(InternalDocumentId item)
    {
        if ((uint)_nextIndex >= (uint)internalDocumentIds.Count)
        {
            return MoveNextRare();
        }

        var itemIndex = internalDocumentIds.BinarySearch(_nextIndex, internalDocumentIds.Count - _nextIndex, item, null);
        if (itemIndex < 0)
        {
            // при побитовом дополнении значение будет указывать на следующий больший индекс
            // "bitwise complement - index is next larger index"
            itemIndex = ~itemIndex;
        }

        _nextIndex = itemIndex;

        if ((uint)_nextIndex >= (uint)internalDocumentIds.Count)
        {
            return MoveNextRare();
        }

        _current = internalDocumentIds[_nextIndex];
        ++_nextIndex;
        return true;
    }

    public bool MoveNext()
    {
        if ((uint)_nextIndex >= (uint)internalDocumentIds.Count)
        {
            return MoveNextRare();
        }

        _current = internalDocumentIds[_nextIndex];
        ++_nextIndex;
        return true;
    }

    private bool MoveNextRare()
    {
        _nextIndex = internalDocumentIds.Count + 1;
        _current = default;
        return false;
    }

    public InternalDocumentId Current => _current;

    object IEnumerator.Current
    {
        get
        {
            if (_nextIndex == 0 || _nextIndex == internalDocumentIds.Count + 1)
            {
                ThrowInvalidOperationException_InvalidOperation_EnumOpCantHappen();
            }

            return Current;
        }
    }

    void IEnumerator.Reset()
    {
        _nextIndex = 0;
        _current = default;
    }

    private static void ThrowInvalidOperationException_InvalidOperation_EnumOpCantHappen()
    {
        throw new InvalidOperationException();
    }
}
