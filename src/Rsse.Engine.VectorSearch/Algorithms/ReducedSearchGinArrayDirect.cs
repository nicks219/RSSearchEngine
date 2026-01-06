using System;
using System.Threading;
using RsseEngine.Contracts;
using RsseEngine.Dto.Common;
using RsseEngine.Indexes;
using RsseEngine.Iterators;
using RsseEngine.Pools;
using RsseEngine.Processor;

namespace RsseEngine.Algorithms;

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
        var idsFromGin = TempStoragePool.InternalDocumentIdListsWithTokenStorage.Get();

        try
        {
            InvertedIndex.CreateNonEmptyDocumentIdsCollection(searchVector, idsFromGin);

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

                        using InternalDocumentReducedScoreIterator documentReducedScoreIterator =
                            new(TempStoragePool, idsFromGin, idsFromGin.Count);

                        MetricsConsumer metricsConsumer = new(searchVector, metricsCalculator, InvertedIndex);

                        documentReducedScoreIterator.Iterate(in metricsConsumer);

                        break;
                    }
            }
        }
        finally
        {
            TempStoragePool.InternalDocumentIdListsWithTokenStorage.Return(idsFromGin);
        }
    }

    private readonly ref struct MetricsConsumer(TokenVector searchVector, IMetricsCalculator metricsCalculator,
        InvertedIndex invertedIndex) : InternalDocumentReducedScoreIterator.IConsumer
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
