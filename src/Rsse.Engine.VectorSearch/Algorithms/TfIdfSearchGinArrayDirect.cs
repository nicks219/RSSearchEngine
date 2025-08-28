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
public readonly ref struct TfIdfSearchGinArrayDirect
{
    public required TempStoragePool TempStoragePool { private get; init; }

    /// <summary>
    /// Поддержка GIN-индекса.
    /// </summary>
    public required InvertedTfIdfIndex InvertedIndex { private get; init; }

    public void FindTfIdf(TokenVector searchVector, TfIdfMetricsCalculator metricsCalculator,
        CancellationToken cancellationToken)
    {
        searchVector = searchVector.DistinctAndGet();

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
                    ProcessSingleIndex(searchVector, metricsCalculator, idsFromGin);
                    break;
                }
                default:
                {
                    Process(searchVector, metricsCalculator, idsFromGin);
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
        List<InternalDocumentIdsWithToken> idsFromGin)
    {
        using InternalDocumentReducedScoreIterator documentReducedScoreIterator =
            new(TempStoragePool, idsFromGin, idsFromGin.Count);

        MetricsConsumer metricsConsumer = new(searchVector, metricsCalculator, InvertedIndex);

        documentReducedScoreIterator.Iterate(in metricsConsumer);
    }

    private void ProcessSingleIndex(TokenVector searchVector, TfIdfMetricsCalculator metricsCalculator,
        List<InternalDocumentIdsWithToken> idsFromGin)
    {
        var metricsConsumer = new MetricsConsumer(searchVector, metricsCalculator, InvertedIndex);

        foreach (var documentId in idsFromGin[0].DocumentIds)
        {
            metricsConsumer.Accept(documentId, 0);
        }
    }

    private readonly ref struct MetricsConsumer(TokenVector searchVector, TfIdfMetricsCalculator metricsCalculator,
        InvertedTfIdfIndex invertedIndex) : InternalDocumentReducedScoreIterator.IConsumer
    {
        public void Accept(InternalDocumentId documentId, int score)
        {
            if (invertedIndex.TryGetOffsetTokenVector(documentId, out var offsetTokenVector, out var externalDocument))
            {
                var tfIdfCalculator = invertedIndex.CreateTfIdfCalculator();

                var metric = tfIdfCalculator.CalculateTfIdf(searchVector, externalDocument.Size, offsetTokenVector);

                metricsCalculator.AppendMetric(metric, externalDocument);
            }
        }
    }
}
