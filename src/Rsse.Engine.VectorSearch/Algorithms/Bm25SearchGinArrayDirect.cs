using System;
using System.Collections.Generic;
using System.Threading;
using RsseEngine.Dto;
using RsseEngine.Indexes;
using RsseEngine.Iterators;
using RsseEngine.Metrics;
using RsseEngine.Pools;

namespace RsseEngine.Algorithms;

/// <summary>
/// Класс с алгоритмом подсчёта TF-IDF метрики.
/// Метрика считается GIN индексе.
/// </summary>
public readonly ref struct Bm25SearchGinArrayDirect
{
    public required TempStoragePool TempStoragePool { private get; init; }

    /// <summary>
    /// Поддержка GIN-индекса.
    /// </summary>
    public required InvertedTfIdfIndex InvertedIndex { private get; init; }

    public readonly InvertedTfIdfIndexPartitions InvertedIndexPartitions { private get; init; }


    public void FindBm25(TokenVector searchVector, TfIdfMetricsCalculator metricsCalculator,
        CancellationToken cancellationToken)
    {
        searchVector = searchVector.DistinctAndGet();

        var idsFromGin = TempStoragePool.InternalDocumentIdListsWithTokenStorage.Get();

        try
        {
            InvertedIndex.GetNonEmptyDocumentIdVectorsToList(searchVector, idsFromGin);

            if (cancellationToken.IsCancellationRequested)
                throw new OperationCanceledException(nameof(ReducedSearchGinArrayDirect));

            var avgDocumentLength = InvertedIndexPartitions.AverageDocumentSize;

            switch (idsFromGin.Count)
            {
                case 0:
                    {
                        break;
                    }
                case 1:
                    {
                        ProcessSingleIndex(searchVector, metricsCalculator, idsFromGin, avgDocumentLength);
                        break;
                    }
                default:
                    {
                        Process(searchVector, metricsCalculator, idsFromGin, avgDocumentLength);
                        break;
                    }
            }
        }
        finally
        {
            TempStoragePool.InternalDocumentIdListsWithTokenStorage.Return(idsFromGin);
        }
    }

    private void Process(TokenVector searchVector, TfIdfMetricsCalculator metricsCalculator,
        List<InternalDocumentIdsWithToken> idsFromGin, double avgDocumentLength)
    {
        using InternalDocumentReducedScoreIterator documentReducedScoreIterator =
            new(TempStoragePool, idsFromGin, idsFromGin.Count);

        MetricsConsumer metricsConsumer = new(searchVector, metricsCalculator, InvertedIndex, avgDocumentLength);

        documentReducedScoreIterator.Iterate(in metricsConsumer);
    }

    private void ProcessSingleIndex(TokenVector searchVector, TfIdfMetricsCalculator metricsCalculator,
        List<InternalDocumentIdsWithToken> idsFromGin, double avgDocumentLength)
    {
        var metricsConsumer = new MetricsConsumer(searchVector, metricsCalculator, InvertedIndex, avgDocumentLength);

        foreach (var documentId in idsFromGin[0].DocumentIds)
        {
            metricsConsumer.Accept(documentId, 0);
        }
    }

    private readonly ref struct MetricsConsumer(TokenVector searchVector, TfIdfMetricsCalculator metricsCalculator,
        InvertedTfIdfIndex invertedIndex, double avgDocumentLength) : InternalDocumentReducedScoreIterator.IConsumer
    {
        public void Accept(InternalDocumentId documentId, int score)
        {
            if (invertedIndex.TryGetOffsetTokenVector(documentId, out var offsetTokenVector, out var externalDocument))
            {
                var bm25Calculator = invertedIndex.CreateBm25Calculator(avgDocumentLength);

                var metric = bm25Calculator.CalculateBm25(searchVector, externalDocument.Size, offsetTokenVector);

                metricsCalculator.AppendMetric(metric, externalDocument);
            }
        }
    }
}
