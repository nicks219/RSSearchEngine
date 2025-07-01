using System;
using System.Collections.Generic;
using System.Threading;
using RsseEngine.Contracts;
using RsseEngine.Dto;
using RsseEngine.Indexes;
using RsseEngine.Processor;

namespace RsseEngine.Algorithms;

/// <summary>
/// Класс с алгоритмом подсчёта сокращенной метрики.
/// Метрика считается на самом GIN индексе и создаётся промежуточный результат для поиска в нём.
/// </summary>
public sealed class ReducedSearchGinOptimized : IReducedSearchProcessor
{
    /// <summary>
    /// Общий индекс: идентификатор-вектор.
    /// </summary>
    public required DirectIndex GeneralDirectIndex { get; init; }

    /// <summary>
    /// Общий инвертированный индекс: токен-идентификаторы.
    /// </summary>
    public required InvertedIndex<DocumentIdSet> GinReduced { get; init; }

    public required GinRelevanceFilter RelevanceFilter { get; init; }

    /// <inheritdoc/>
    public void FindReduced(TokenVector searchVector, IMetricsCalculator metricsCalculator, CancellationToken cancellationToken)
    {
        if (!RelevanceFilter.Enabled)
        {
            // убираем дубликаты слов для intersect - это меняет результаты поиска (тексты типа "казино казино казино")
            searchVector = searchVector.DistinctAndGet();

            // сразу посчитаем на GIN метрики intersect.count для актуальных идентификаторов
            var comparisonScoresReduced = new Dictionary<DocumentId, int>();
            foreach (var token in searchVector)
            {
                if (!GinReduced.TryGetIdentifiers(token, out var ids))
                {
                    continue;
                }

                foreach (var docId in ids)
                {
                    if (!comparisonScoresReduced.TryAdd(docId, 1))
                    {
                        // Это метрика intersect.count.
                        comparisonScoresReduced[docId]++;
                    }
                }
            }

            if (cancellationToken.IsCancellationRequested)
                throw new OperationCanceledException(nameof(ReducedSearchGinOptimized));

            // поиск в векторе reduced
            foreach (var (docId, comparisonScore) in comparisonScoresReduced)
            {
                var reducedTargetVector = GeneralDirectIndex[docId].Reduced;

                metricsCalculator.AppendReduced(comparisonScore, searchVector, docId, reducedTargetVector);
            }
        }
        else
        {
            // убираем дубликаты слов для intersect - это меняет результаты поиска (тексты типа "казино казино казино")
            searchVector = searchVector.DistinctAndGet();

            var filteredDocuments = RelevanceFilter.ProcessToSet(GinReduced, searchVector);

            // сразу посчитаем на GIN метрики intersect.count для актуальных идентификаторов
            var comparisonScoresReduced = new Dictionary<DocumentId, int>();
            foreach (var token in searchVector)
            {
                if (!GinReduced.TryGetIdentifiers(token, out var ids))
                {
                    continue;
                }

                foreach (var docId in ids)
                {
                    if (!filteredDocuments.Contains(docId))
                    {
                        continue;
                    }

                    if (!comparisonScoresReduced.TryAdd(docId, 1))
                    {
                        // Это метрика intersect.count.
                        comparisonScoresReduced[docId]++;
                    }
                }
            }

            if (cancellationToken.IsCancellationRequested)
                throw new OperationCanceledException(nameof(ReducedSearchGinOptimized));

            // поиск в векторе reduced
            foreach (var (docId, comparisonScore) in comparisonScoresReduced)
            {
                var reducedTargetVector = GeneralDirectIndex[docId].Reduced;

                metricsCalculator.AppendReduced(comparisonScore, searchVector, docId, reducedTargetVector);
            }
        }
    }
}
