using System;
using System.Threading;
using SimpleEngine.Contracts;
using SimpleEngine.Dto.Common;
using SimpleEngine.Indexes;
using SimpleEngine.Iterators;
using SimpleEngine.Pools;
using SimpleEngine.Processor;

namespace SimpleEngine.Algorithms;

/// <summary>
/// Класс с алгоритмом подсчёта сокращенной метрики.
/// Метрика считается GIN индексе, применены дополнительные оптимизации.
/// </summary>
public readonly ref struct ReducedSearchGinArrayDirect : IReducedSearchProcessor
{
    public required TempStoragePool TempStoragePool { private get; init; }

    /// <summary>
    /// Поддержка GIN-индекса.
    /// </summary>
    public required InvertedIndex InvertedIndex { private get; init; }

    /// <inheritdoc/>
    public void FindReduced(TokenVector searchVector, IMetricsCalculator metricsCalculator,
        CancellationToken cancellationToken)
    {
        var idsFromGin = TempStoragePool.InternalIdsWithTokenCollections.Get();

        try
        {
            InvertedIndex.FillWithNonEmptyDocumentIds(searchVector, idsFromGin);

            switch (idsFromGin.Count)
            {
                case 0:
                    {
                        break;
                    }
                case 1:
                    {
                        foreach (var documentId in idsFromGin[0].DocumentIds)
                        {
                            if (InvertedIndex.TryGetPositionVector(documentId, out _, out var externalDocument))
                            {
                                const int metric = 1;
                                metricsCalculator.AppendReducedMetric(metric, searchVector, externalDocument);
                            }
                        }

                        break;
                    }
                default:
                    {
                        if (cancellationToken.IsCancellationRequested)
                            throw new OperationCanceledException(nameof(ReducedSearchGinArrayDirect));

                        using DocumentIdsScoringIterator documentReducedScoreIterator =
                            new(TempStoragePool, idsFromGin, idsFromGin.Count);

                        MetricsConsumer metricsConsumer = new(searchVector, metricsCalculator, InvertedIndex);

                        documentReducedScoreIterator.AppendReducedMetric(in metricsConsumer);

                        break;
                    }
            }
        }
        finally
        {
            TempStoragePool.InternalIdsWithTokenCollections.Return(idsFromGin);
        }
    }

    private readonly ref struct MetricsConsumer(TokenVector searchVector, IMetricsCalculator metricsCalculator,
        InvertedIndex invertedIndex) : DocumentIdsScoringIterator.IMetricsConsumer
    {
        public void Accept(InternalDocumentId documentId, int score)
        {
            if (invertedIndex.TryGetPositionVector(documentId, out _, out var externalDocument))
            {
                metricsCalculator.AppendReducedMetric(score, searchVector, externalDocument);
            }
        }
    }
}
