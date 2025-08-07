using System;
using System.Threading;
using RsseEngine.Contracts;
using RsseEngine.Dto;
using RsseEngine.Indexes;
using RsseEngine.Iterators;
using RsseEngine.Pools;

namespace RsseEngine.Algorithms;

/// <summary>
/// Класс с алгоритмом подсчёта сокращенной метрики.
/// Метрика считается GIN индексе, применены дополнительные оптимизации.
/// </summary>
public sealed class ReducedSearchGinMerge : IReducedSearchProcessor
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

    /// <inheritdoc/>
    public void FindReduced(TokenVector searchVector, IMetricsCalculator metricsCalculator,
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
                            throw new OperationCanceledException(nameof(ReducedSearchGinMerge));

                        using DocumentReducedScoreIterator documentReducedScoreIterator =
                            new(TempStoragePool, idsFromGin, idsFromGin.Count);

                        MetricsConsumer metricsConsumer = new(searchVector, metricsCalculator, GeneralDirectIndex);

                        documentReducedScoreIterator.Iterate(in metricsConsumer);

                        break;
                    }
            }
        }
        finally
        {
            TempStoragePool.ReturnDocumentIdCollectionList(idsFromGin);
        }
    }

    private readonly ref struct MetricsConsumer(TokenVector searchVector, IMetricsCalculator metricsCalculator,
        DirectIndex generalDirectIndex) : DocumentReducedScoreIterator.IConsumer
    {
        public void Accept(DocumentId documentId, int score)
        {
            metricsCalculator.AppendReduced(score, searchVector, documentId, generalDirectIndex);
        }
    }
}
