using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using RsseEngine.Contracts;

namespace RsseEngine.Dto;

public ref struct DocumentIdCollectionEnumerator<TDocumentIdCollection> : IEnumerator<DocumentId>
    where TDocumentIdCollection : struct, IDocumentIdCollection<TDocumentIdCollection>
{
    private HashSet<DocumentId>.Enumerator _setEnumerator;
    private List<DocumentId>.Enumerator _listEnumerator;

    public DocumentIdCollectionEnumerator(HashSet<DocumentId> hashSet)
    {
        _setEnumerator = hashSet.GetEnumerator();
    }

    public DocumentIdCollectionEnumerator(List<DocumentId> list)
    {
        _listEnumerator = list.GetEnumerator();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool MoveNext()
    {
        if (typeof(TDocumentIdCollection) == typeof(DocumentIdSet))
        {
            return _setEnumerator.MoveNext();
        }

        if (typeof(TDocumentIdCollection) == typeof(DocumentIdList))
        {
            return _listEnumerator.MoveNext();
        }

        ThrowNotSupportedException();
        return false;
    }

    public DocumentId Current
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            if (typeof(TDocumentIdCollection) == typeof(DocumentIdSet))
            {
                return _setEnumerator.Current;
            }

            if (typeof(TDocumentIdCollection) == typeof(DocumentIdList))
            {
                return _listEnumerator.Current;
            }

            ThrowNotSupportedException();
            return default;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Dispose()
    {
        if (typeof(TDocumentIdCollection) == typeof(DocumentIdSet))
        {
            _setEnumerator.Dispose();
            return;
        }

        if (typeof(TDocumentIdCollection) == typeof(DocumentIdList))
        {
            _listEnumerator.Dispose();
            return;
        }

        ThrowNotSupportedException();
    }

    object IEnumerator.Current => Current;

    void IEnumerator.Reset()
    {
        if (typeof(TDocumentIdCollection) == typeof(DocumentIdSet))
        {
            IEnumerator enumerator = _setEnumerator;
            enumerator.Reset();
            _setEnumerator = (HashSet<DocumentId>.Enumerator)enumerator;
            return;
        }

        if (typeof(TDocumentIdCollection) == typeof(DocumentIdList))
        {
            IEnumerator enumerator = _setEnumerator;
            enumerator.Reset();
            _listEnumerator = (List<DocumentId>.Enumerator)enumerator;
            return;
        }

        ThrowNotSupportedException();
    }

    private static void ThrowNotSupportedException()
    {
        throw new NotSupportedException($"[{nameof(TDocumentIdCollection)}] is not supported.");
    }
}
