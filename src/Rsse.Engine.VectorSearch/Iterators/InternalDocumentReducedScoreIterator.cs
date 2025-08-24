using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using RsseEngine.Dto;
using RsseEngine.Pools;
using RsseEngine.Processor;

namespace RsseEngine.Iterators;

public readonly ref struct InternalDocumentReducedScoreIterator : IDisposable
{
    public interface IConsumer
    {
        public void Accept(InternalDocumentId documentId, int score);
    }

    private readonly TempStoragePool _tempStoragePool;

    private readonly List<InternalDocumentListEnumerator> _list;

    public InternalDocumentReducedScoreIterator(TempStoragePool tempStoragePool,
        List<InternalDocumentIdsWithToken> sortedIds, int filteredTokensCount)
    {
        _tempStoragePool = tempStoragePool;
        _list = tempStoragePool.ListInternalEnumeratorListsStorage.Get();

        for (var index = 0; index < filteredTokensCount; index++)
        {
            var docIdVector = sortedIds[index];
            _list.Add(docIdVector.DocumentIds.CreateDocumentListEnumerator());
        }

        for (var index = 0; index < _list.Count; index++)
        {
            CollectionsMarshal.AsSpan(_list)[index].MoveNext();
        }
    }

    public void Dispose()
    {
        _tempStoragePool.ListInternalEnumeratorListsStorage.Return(_list);
    }

    public void Iterate<TConsumer>(in TConsumer consumer)
        where TConsumer : IConsumer, allows ref struct
    {
        while (_list.Count > 1)
        {
            MergeAlgorithm.FindMin(_list, out var minI0, out var docId0, out var docId1);

        START:

            if (docId0.Value < docId1.Value)
            {
                consumer.Accept(docId0, 1);

                ref var enumeratorI = ref CollectionsMarshal.AsSpan(_list)[minI0];
                if (!enumeratorI.MoveNext())
                {
                    _list.RemoveAt(minI0);
                }
                else
                {
                    docId0 = enumeratorI.Current;
                    goto START;
                }
            }
            else if (docId0 == docId1)
            {
                int score = 0;

                for (int i = _list.Count - 1; i >= 0; i--)
                {
                    ref var enumeratorI = ref CollectionsMarshal.AsSpan(_list)[i];
                    if (docId0 == enumeratorI.Current)
                    {
                        score++;
                        if (!enumeratorI.MoveNext())
                        {
                            _list.RemoveAt(i);
                        }
                    }
                }

                consumer.Accept(docId0, score);
            }
        }

        if (_list.Count == 1)
        {
            var enumerator0 = _list[0];
            do
            {
                consumer.Accept(enumerator0.Current, 1);
            } while (enumerator0.MoveNext());
        }
    }
}
