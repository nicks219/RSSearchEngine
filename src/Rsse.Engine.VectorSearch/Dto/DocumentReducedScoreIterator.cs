using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using RsseEngine.Pools;

namespace RsseEngine.Dto;

public readonly ref struct DocumentReducedScoreIterator : IDisposable
{
    public interface IConsumer
    {
        public void Accept(DocumentId documentId, int score);
    }

    private readonly TempStoragePool _tempStoragePool;

    private readonly List<DocumentListEnumerator> _list;

    public DocumentReducedScoreIterator(TempStoragePool tempStoragePool,
        List<DocumentIdList> sortedIds, int filteredTokensCount)
    {
        _tempStoragePool = tempStoragePool;
        _list = tempStoragePool.ListEnumeratorListsStorage.Get();

        for (var index = 0; index < filteredTokensCount; index++)
        {
            var docIdVector = sortedIds[index];
            _list.Add(docIdVector.CreateDocumentListEnumerator());
        }

        for (var index = 0; index < _list.Count; index++)
        {
            CollectionsMarshal.AsSpan(_list)[index].MoveNext();
        }
    }

    public void Dispose()
    {
        _tempStoragePool.ListEnumeratorListsStorage.Return(_list);
    }

    public void Iterate<TConsumer>(in TConsumer consumer)
        where TConsumer : IConsumer, allows ref struct
    {
        if (_list.Count == 0)
        {
            return;
        }

        if (_list.Count == 1)
        {
            var enumerator0 = _list[0];
            do
            {
                consumer.Accept(enumerator0.Current, 1);
            } while (enumerator0.MoveNext());

            return;
        }

        do
        {
            FindMin(_list, out var minI0, out var docId0, out var docId1);

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
        } while (_list.Count > 1);

        if (_list.Count == 1)
        {
            var enumerator0 = _list[0];
            do
            {
                consumer.Accept(enumerator0.Current, 1);
            } while (enumerator0.MoveNext());
        }
    }

    private static void FindMin(List<DocumentListEnumerator> list, out int minI0, out DocumentId min0, out DocumentId min1)
    {
        minI0 = 0;
        int minI1 = 1;
        min0 = list[minI0].Current;
        min1 = list[minI1].Current;

        if (min0.Value > min1.Value)
        {
            (minI0, minI1) = (minI1, minI0);
            (min0, min1) = (min1, min0);
        }

        for (int i = 2; i < list.Count; i++)
        {
            var index = i;
            var documentId = list[index].Current;

            if (documentId.Value < min0.Value)
            {
                min1 = min0;
                //minI1 = minI0;
                min0 = documentId;
                minI0 = index;
            }
            else if (documentId.Value < min1.Value)
            {
                min1 = documentId;
                //minI1 = index;
            }
        }
    }
}
