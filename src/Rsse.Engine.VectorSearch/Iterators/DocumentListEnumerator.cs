using System;
using System.Collections;
using System.Collections.Generic;
using RsseEngine.Dto;

namespace RsseEngine.Iterators;

public struct DocumentListEnumerator : IEnumerator<DocumentId>
{
    private readonly List<DocumentId> _list;
    private int _index;
    private DocumentId _current;

    public DocumentListEnumerator(List<DocumentId> list)
    {
        _list = list;
        _index = 0;
        _current = default;
    }

    public void Dispose()
    {
    }

    public bool MoveNextBinarySearch(DocumentId item)
    {
        var list = _list;

        if ((uint)_index >= (uint)list.Count)
            return MoveNextRare();

        int itemIndex = list.BinarySearch(_index, list.Count - _index, item, null);
        if (itemIndex < 0)
        {
            // при побитовом дополнении значение будет указывать на следующий больший индекс
            // "bitwise complement - index is next larger index"
            itemIndex = ~itemIndex;
        }

        _index = itemIndex;

        if ((uint)_index >= (uint)list.Count)
            return MoveNextRare();

        _current = list[_index];
        ++_index;
        return true;
    }

    public bool MoveNext()
    {
        var list = _list;

        if ((uint)_index >= (uint)list.Count)
            return MoveNextRare();

        _current = list[_index];
        ++_index;
        return true;
    }

    private bool MoveNextRare()
    {
        _index = _list.Count + 1;
        _current = default;
        return false;
    }

    public DocumentId Current => _current;

    object IEnumerator.Current
    {
        get
        {
            if (_index == 0 || _index == _list.Count + 1)
                ThrowInvalidOperationException_InvalidOperation_EnumOpCantHappen();

            return Current;
        }
    }

    void IEnumerator.Reset()
    {
        _index = 0;
        _current = default;
    }

    private static void ThrowInvalidOperationException_InvalidOperation_EnumOpCantHappen()
    {
        throw new InvalidOperationException();
    }
}
