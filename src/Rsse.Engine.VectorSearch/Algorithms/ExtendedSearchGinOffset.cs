using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using RsseEngine.Contracts;
using RsseEngine.Dto;
using RsseEngine.Indexes;
using RsseEngine.Iterators;
using RsseEngine.Pools;
using RsseEngine.Processor;

namespace RsseEngine.Algorithms;

/// <summary>
/// Класс с алгоритмом подсчёта расширенной метрики.
/// Пространство поиска формируется с помощью GIN индекса, применены дополнительные оптимизации.
/// </summary>
public sealed class ExtendedSearchGinOffset : IExtendedSearchProcessor
{
    public required TempStoragePool TempStoragePool { private get; init; }

    /// <summary>
    /// Общий индекс: идентификатор-вектор.
    /// </summary>
    public required DirectIndex GeneralDirectIndex { private get; init; }

    /// <summary>
    /// Общий инвертированный индекс: токен-идентификаторы.
    /// </summary>
    public required InvertedOffsetIndex GinExtended { private get; init; }

    /// <inheritdoc/>
    public void FindExtended(TokenVector searchVector, IMetricsCalculator metricsCalculator,
        CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
            throw new OperationCanceledException(nameof(ExtendedSearchGinOffset));

        var enumerators = TempStoragePool.TokenOffsetEnumeratorListsStorage.Get();

        try
        {
            CreateEnumerators(searchVector, enumerators);

            switch (enumerators.Count)
            {
                case 0:
                    {
                        break;
                    }
                case 1:
                    {
                        ProcessSingle(searchVector, metricsCalculator, enumerators);
                        break;
                    }
                default:
                    {
                        ProcessMulti(searchVector, metricsCalculator, enumerators);
                        break;
                    }
            }
        }
        finally
        {
            TempStoragePool.TokenOffsetEnumeratorListsStorage.Return(enumerators);
        }
    }

    private void CreateEnumerators(TokenVector searchVector, List<TokenOffsetEnumerator> enumerators)
    {
        for (var searchStartIndex = 0; searchStartIndex < searchVector.Count; searchStartIndex++)
        {
            var token = searchVector.ElementAt(searchStartIndex);

            if (!GinExtended.TryGetNonEmptyDocumentIdVector(token, out var documentIds))
            {
                continue;
            }

            var enumerator = documentIds.DocumentIds.CreateDocumentListEnumerator();

            if (enumerator.MoveNext())
            {
                enumerators.Add(new TokenOffsetEnumerator(documentIds, enumerator));
            }
        }
    }

    private void ProcessSingle(TokenVector searchVector, IMetricsCalculator metricsCalculator,
        List<TokenOffsetEnumerator> enumerators)
    {
        var enumerator = enumerators[0];

        do
        {
            var documentId = enumerator.Current;
            const int metric = 1;

            if (GinExtended.TryGetExternalDocumentId(documentId, out var externalDocumentId))
            {
                metricsCalculator.AppendExtended(metric, searchVector, externalDocumentId, GeneralDirectIndex);
            }

        } while (enumerator.MoveNext());
    }

    private void ProcessMulti(TokenVector searchVector, IMetricsCalculator metricsCalculator,
        List<TokenOffsetEnumerator> enumerators)
    {
        do
        {
            MergeAlgorithm.FindMin(enumerators, out var minI0, out var docId0, out var docId1);

            START:
            if (docId0.Value < docId1.Value)
            {
                const int metric = 1;

                if (GinExtended.TryGetExternalDocumentId(docId0, out var externalDocumentId))
                {
                    metricsCalculator.AppendExtended(metric, searchVector, externalDocumentId, GeneralDirectIndex);
                }

                ref var enumerator = ref CollectionsMarshal.AsSpan(enumerators)[minI0];
                if (!enumerator.MoveNext())
                {
                    enumerators.RemoveAt(minI0);
                }
                else
                {
                    docId0 = enumerator.Current;
                    goto START;
                }
            }
            else if (docId0 == docId1)
            {
                var documentId = docId0;
                var position = -1;
                var metric = 0;

                for (var i = 0; i < enumerators.Count; i++)
                {
                    ref var enumerator = ref CollectionsMarshal.AsSpan(enumerators)[i];

                    var documentId1 = enumerator.Current;

                    if (documentId1.Value < documentId.Value)
                    {
                        if (!enumerator.MoveNextBinarySearch(documentId))
                        {
                            enumerators.RemoveAt(i);
                            i--;
                            continue;
                        }

                        documentId1 = enumerator.Current;
                    }

                    if (documentId1 == documentId &&
                        enumerator.FindNextPosition(ref position))
                    {
                        metric++;

                        if (!enumerator.MoveNext())
                        {
                            enumerators.RemoveAt(i);
                            i--;
                        }
                    }
                }

                if (position >= 0)
                {
                    if (GinExtended.TryGetExternalDocumentId(documentId, out var externalDocumentId))
                    {
                        metricsCalculator.AppendExtended(metric, searchVector, externalDocumentId, GeneralDirectIndex);
                    }
                }
            }
        } while (enumerators.Count > 1);

        if (enumerators.Count == 1)
        {
            ProcessSingle(searchVector, metricsCalculator, enumerators);
        }
    }
}
