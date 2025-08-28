using System;
using System.Collections.Generic;
using System.Threading;
using RsseEngine.Contracts;
using RsseEngine.Dto;
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
            InvertedIndex.GetNonEmptyDocumentIdVectorsToList(searchVector, idsFromGin);

            if (cancellationToken.IsCancellationRequested)
                throw new OperationCanceledException(nameof(ReducedSearchGinArrayDirect));

            switch (idsFromGin.Count)
            {
                case 0:
                    {
                        break;
                    }
                case 1:
                    {
                        metricsCalculator.AppendReducedMetricsFromSingleIndex(searchVector,
                            InvertedIndex, idsFromGin[0].DocumentIds);

                        break;
                    }
                default:
                {
                    Process(idsFromGin, searchVector, metricsCalculator);

                    break;
                }
            }
        }
        finally
        {
            TempStoragePool.InternalDocumentIdListsWithTokenStorage.Return(idsFromGin);
        }
    }

    private void Process(List<InternalDocumentIdsWithToken> idsFromGin,
        TokenVector searchVector, IMetricsCalculator metricsCalculator)
    {
        using InternalDocumentReducedScoreIterator documentReducedScoreIterator =
            new(TempStoragePool, idsFromGin, idsFromGin.Count);

        MetricsConsumer metricsConsumer = new(searchVector, metricsCalculator, InvertedIndex);

        documentReducedScoreIterator.Iterate(in metricsConsumer);
    }

    private readonly ref struct MetricsConsumer(TokenVector searchVector, IMetricsCalculator metricsCalculator,
        InvertedIndex invertedIndex) : InternalDocumentReducedScoreIterator.IConsumer
    {
        public void Accept(InternalDocumentId documentId, int score)
        {
            if (invertedIndex.TryGetOffsetTokenVector(documentId, out _, out var externalDocument))
            {
                metricsCalculator.AppendReducedMetric(score, searchVector, externalDocument);
            }
        }
    }
}
