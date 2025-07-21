using System;
using System.Threading;
using RsseEngine.Contracts;
using RsseEngine.Dto;
using RsseEngine.Indexes;
using RsseEngine.Pools;
using RsseEngine.Processor;

namespace RsseEngine.Algorithms;

/// <summary>
/// Класс с алгоритмом подсчёта расширенной метрики.
/// Пространство поиска формируется с помощью GIN индекса.
/// </summary>
public sealed class ReducedSearchGinFilter<TDocumentIdCollection> : IReducedSearchProcessor
    where TDocumentIdCollection : struct, IDocumentIdCollection<TDocumentIdCollection>
{
    public required TempStoragePool TempStoragePool { private get; init; }

    /// <summary>
    /// Общий индекс: идентификатор-вектор.
    /// </summary>
    public required DirectIndex GeneralDirectIndex { private get; init; }

    /// <summary>
    /// Общий инвертированный индекс: токен-идентификаторы.
    /// </summary>
    public required InvertedIndex<TDocumentIdCollection> GinReduced { private get; init; }

    public required GinRelevanceFilter RelevanceFilter { private get; init; }

    /// <inheritdoc/>
    public void FindReduced(TokenVector searchVector, IMetricsCalculator metricsCalculator, CancellationToken cancellationToken)
    {
        // убираем дубликаты слов для intersect - это меняет результаты поиска (тексты типа "казино казино казино")
        searchVector = searchVector.DistinctAndGet();

        var comparisonScores = TempStoragePool.ScoresStorage.Get();
        var sortedIds = TempStoragePool.GetDocumentIdCollectionList<TDocumentIdCollection>();

        try
        {
            if (!RelevanceFilter.FindFilteredDocumentsReduced(GinReduced, searchVector, sortedIds,
                    out _, comparisonScores))
            {
                return;
            }

            if (cancellationToken.IsCancellationRequested)
                throw new OperationCanceledException(nameof(ReducedSearchGinFilter<TDocumentIdCollection>));

            // поиск в векторе reduced
            foreach (var (documentId, _) in comparisonScores)
            {
                var reducedTargetVector = GeneralDirectIndex[documentId].Reduced;
                var comparisonScore = ScoreCalculator.ComputeUnordered(reducedTargetVector, searchVector);

                // Для расчета метрик необходимо учитывать размер оригинальной заметки.
                metricsCalculator.AppendReduced(comparisonScore, searchVector, documentId, reducedTargetVector);
            }
        }
        finally
        {
            TempStoragePool.ReturnDocumentIdCollectionList(sortedIds);
            TempStoragePool.ScoresStorage.Return(comparisonScores);
        }
    }
}
