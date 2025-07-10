using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using RsseEngine.Contracts;
using RsseEngine.Dto;
using RsseEngine.Indexes;
using RsseEngine.Pools;
using RsseEngine.Processor;

namespace RsseEngine.Algorithms;

/// <summary>
/// Класс с алгоритмом подсчёта сокращенной метрики.
/// Метрика считается GIN индексе, применены дополнительные оптимизации.
/// </summary>
public sealed class ReducedSearchGinMerge1 : IReducedSearchProcessor
{
    public required TempStoragePool TempStoragePool { private get; init; }

    /// <summary>
    /// Индекс для всех токенизированных заметок.
    /// </summary>
    public required DirectIndex GeneralDirectIndex { get; init; }

    /// <summary>
    /// Поддержка GIN-индекса.
    /// </summary>
    public required InvertedIndex<DocumentIdList> GinReduced { get; init; }

    public required GinRelevanceFilter RelevanceFilter { private get; init; }

    public required bool EnableRelevanceFilter { private get; init; }

    /// <inheritdoc/>
    public void FindReduced(TokenVector searchVector, IMetricsCalculator metricsCalculator,
        CancellationToken cancellationToken)
    {
        if (EnableRelevanceFilter)
        {
            FindReducedWithFilter(searchVector, metricsCalculator, cancellationToken);
        }
        else
        {
            FindReducedNoFilter(searchVector, metricsCalculator, cancellationToken);
        }
    }

    private void FindReducedNoFilter(TokenVector searchVector, IMetricsCalculator metricsCalculator,
        CancellationToken cancellationToken)
    {
        // убираем дубликаты слов для intersect - это меняет результаты поиска (тексты типа "казино казино казино")
        searchVector = searchVector.DistinctAndGet();

        var idsFromGin = TempStoragePool.GetDocumentIdCollectionList<DocumentIdList>();

        try
        {
            GinReduced.GetNonEmptyDocumentIdVectorsToList(searchVector, idsFromGin);

            switch (idsFromGin.Count)
            {
                case 0:
                    {
                        break;
                    }
                case 1:
                    {
                        foreach (var documentId in idsFromGin[0])
                        {
                            metricsCalculator.AppendReduced(1, searchVector, documentId, GeneralDirectIndex);
                        }

                        break;
                    }
                default:
                    {
                        if (cancellationToken.IsCancellationRequested)
                            throw new OperationCanceledException(nameof(ReducedSearchGinMerge1));

                        CreateExtendedSearchSpace(searchVector, metricsCalculator, idsFromGin);

                        break;
                    }
            }
        }
        finally
        {
            TempStoragePool.ReturnDocumentIdCollectionList(idsFromGin);
        }
    }

    private void FindReducedWithFilter(TokenVector searchVector, IMetricsCalculator metricsCalculator,
        CancellationToken cancellationToken)
    {
        // убираем дубликаты слов для intersect - это меняет результаты поиска (тексты типа "казино казино казино")
        searchVector = searchVector.DistinctAndGet();

        var comparisonScores = TempStoragePool.ScoresStorage.Get();
        var idsFromGin = TempStoragePool.GetDocumentIdCollectionList<DocumentIdList>();
        var idsFromGin2 = TempStoragePool.GetDocumentIdCollectionList<DocumentIdList>();

        try
        {
            if (!RelevanceFilter.FindFilteredDocumentsReducedMerge(GinReduced, searchVector, comparisonScores,
                    idsFromGin, idsFromGin2))
            {
                return;
            }

            switch (idsFromGin.Count)
            {
                case 0:
                    {
                        break;
                    }
                case 1:
                    {
                        foreach (var documentId in idsFromGin[0])
                        {
                            metricsCalculator.AppendReduced(1, searchVector, documentId, GeneralDirectIndex);
                        }

                        break;
                    }
                default:
                    {
                        if (cancellationToken.IsCancellationRequested)
                            throw new OperationCanceledException(nameof(ReducedSearchGinMerge1));

                        CreateExtendedSearchSpace(searchVector, metricsCalculator, idsFromGin2);

                        break;
                    }
            }
        }
        finally
        {
            TempStoragePool.ReturnDocumentIdCollectionList(idsFromGin2);
            TempStoragePool.ReturnDocumentIdCollectionList(idsFromGin);
            TempStoragePool.ScoresStorage.Return(comparisonScores);
        }
    }

    private void CreateExtendedSearchSpace(TokenVector searchVector, IMetricsCalculator metricsCalculator,
        List<DocumentIdList> idsFromGin)
    {
        var list = new List<List<DocumentId>.Enumerator>();

        foreach (var docIdVector in idsFromGin)
        {
            list.Add(docIdVector.GetRawEnumerator());
        }

        for (var index = 0; index < list.Count; index++)
        {
            CollectionsMarshal.AsSpan(list)[index].MoveNext();
        }

        do
        {
            FindMin(list, 0);
            FindMin(list, 1);

            var docId1 = list[1].Current;
            START:
            var docId0 = list[0].Current;

            if (docId0.Value < docId1.Value)
            {
                AppendMetric(metricsCalculator, 1, searchVector, docId0);

                if (!CollectionsMarshal.AsSpan(list)[0].MoveNext())
                {
                    list.RemoveAt(0);
                }
                else
                {
                    goto START;
                }
            }
            else if (docId0 == docId1)
            {
                int score = 0;

                for (int i = list.Count - 1; i >= 0; i--)
                {
                    ref var enumeratorI = ref CollectionsMarshal.AsSpan(list)[i];
                    if (docId0 == enumeratorI.Current)
                    {
                        score++;
                        if (!enumeratorI.MoveNext())
                        {
                            list.RemoveAt(i);
                        }
                    }
                }

                AppendMetric(metricsCalculator, score, searchVector, docId0);
            }
        } while (list.Count > 1);

        if (list.Count > 0)
        {
            var enumerator0 = list[0];
            do
            {
                AppendMetric(metricsCalculator, 1, searchVector, enumerator0.Current);
            } while (enumerator0.MoveNext());
        }
    }

    private void AppendMetric(IMetricsCalculator metricsCalculator, int comparisonScore,
        TokenVector searchVector, DocumentId documentId)
    {
        if (EnableRelevanceFilter)
        {
            var tokensLine = GeneralDirectIndex[documentId];
            var reducedTargetVector = tokensLine.Reduced;
            int metric = ScoreCalculator.ComputeUnordered(reducedTargetVector, searchVector);

            metricsCalculator.AppendReduced(metric, searchVector, documentId, reducedTargetVector);
        }
        else
        {
            metricsCalculator.AppendReduced(comparisonScore, searchVector, documentId, GeneralDirectIndex);
        }
    }

    private static void FindMin(List<List<DocumentId>.Enumerator> list, int start)
    {
        var minI = start;
        var min = list[start].Current;

        for (int i = list.Count - 1; i > start; i--)
        {
            var documentId = list[i].Current;
            if (documentId.Value < min.Value)
            {
                min = documentId;
                minI = i;
            }
        }

        if (minI > start)
        {
            (list[start], list[minI]) = (list[minI], list[start]);
        }
    }
}
