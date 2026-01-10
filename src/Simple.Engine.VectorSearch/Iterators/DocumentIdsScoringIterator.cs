using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using SimpleEngine.Dto.Common;
using SimpleEngine.Pools;
using SimpleEngine.Processor;

namespace SimpleEngine.Iterators;

/// <summary>
/// Итератор используется для подсчета метрик в reduced поиске.
/// </summary>
public readonly ref struct DocumentIdsScoringIterator : IDisposable
{
    public interface IMetricsConsumer
    {
        void Accept(InternalDocumentId documentId, int score);
    }

    private readonly TempStoragePool _tempStoragePool;

    private readonly List<DocumentIdsEnumerator> _enumerators;

    public DocumentIdsScoringIterator(
        TempStoragePool tempStoragePool,
        List<InternalDocumentIdsWithToken> sortedIds,
        int filteredTokensCount)
    {
        _tempStoragePool = tempStoragePool;
        _enumerators = tempStoragePool.InternalEnumeratorCollections.Get();

        for (var index = 0; index < filteredTokensCount; index++)
        {
            var docIdVector = sortedIds[index];
            _enumerators.Add(docIdVector.DocumentIds.CreateDocumentListEnumerator());
        }

        for (var index = 0; index < _enumerators.Count; index++)
        {
            CollectionsMarshal.AsSpan(_enumerators)[index].MoveNext();
        }
    }

    public void Dispose()
    {
        _tempStoragePool.InternalEnumeratorCollections.Return(_enumerators);
    }

    public void IterateToObtainReducedMetric<TConsumer>(in TConsumer consumer) where TConsumer : IMetricsConsumer, allows ref struct
    {
        while (_enumerators.Count > 1)
        {
            MergeHelpers.FindTwoMinimumIds(_enumerators, out var firstMinIndex, out var firstMinId, out var secondMinId);

        START:

            if (firstMinId < secondMinId)
            {
                consumer.Accept(firstMinId, 1);

                ref var enumeratorFirst = ref CollectionsMarshal.AsSpan(_enumerators)[firstMinIndex];
                if (!enumeratorFirst.MoveNext())
                {
                    _enumerators.RemoveAt(firstMinIndex);
                }
                else
                {
                    firstMinId = enumeratorFirst.Current;
                    goto START;
                }
            }
            else if (firstMinId == secondMinId)
            {
                var score = 0;

                for (var index = _enumerators.Count - 1; index >= 0; index--)
                {
                    ref var enumeratorAtIndex = ref CollectionsMarshal.AsSpan(_enumerators)[index];
                    if (firstMinId == enumeratorAtIndex.Current)
                    {
                        score++;
                        if (!enumeratorAtIndex.MoveNext())
                        {
                            _enumerators.RemoveAt(index);
                        }
                    }
                }

                consumer.Accept(firstMinId, score);
            }
        }

        if (_enumerators.Count == 1)
        {
            var enumeratorSingle = _enumerators[0];
            do
            {
                consumer.Accept(enumeratorSingle.Current, 1);
            } while (enumeratorSingle.MoveNext());
        }
    }
}
