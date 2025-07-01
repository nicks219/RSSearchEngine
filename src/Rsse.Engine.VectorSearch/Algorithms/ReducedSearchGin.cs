using System;
using System.Threading;
using RsseEngine.Contracts;
using RsseEngine.Dto;
using RsseEngine.Indexes;
using RsseEngine.Processor;

namespace RsseEngine.Algorithms;

/// <summary>
/// Класс с алгоритмом подсчёта сокращенной метрики.
/// Метрика считается с помощью GIN индекса в процессе поиска.
/// </summary>
public sealed class ReducedSearchGin : IReducedSearchProcessor
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

            if (cancellationToken.IsCancellationRequested)
                throw new OperationCanceledException(nameof(ReducedSearchGin));

            // поиск в векторе reduced
            foreach (var (docId, tokenLine) in GeneralDirectIndex)
            {
                var reducedTargetVector = tokenLine.Reduced;
                var comparisonScore = 0;
                foreach (var token in searchVector)
                {
                    if (GinReduced.TryGetIdentifiers(token, out var reducedTokens) && reducedTokens.Contains(docId))
                    {
                        comparisonScore++;
                    }
                }

                metricsCalculator.AppendReduced(comparisonScore, searchVector, docId, reducedTargetVector);
            }
        }
        else
        {
            // убираем дубликаты слов для intersect - это меняет результаты поиска (тексты типа "казино казино казино")
            searchVector = searchVector.DistinctAndGet();

            var filteredDocuments = RelevanceFilter.ProcessToSet(GinReduced, searchVector);

            if (cancellationToken.IsCancellationRequested)
                throw new OperationCanceledException(nameof(ReducedSearchGin));

            // поиск в векторе reduced
            foreach (var (docId, tokenLine) in GeneralDirectIndex)
            {
                if (!filteredDocuments.Contains(docId))
                {
                    continue;
                }

                var reducedTargetVector = tokenLine.Reduced;
                var comparisonScore = 0;
                foreach (var token in searchVector)
                {
                    if (GinReduced.TryGetIdentifiers(token, out var reducedTokens) && reducedTokens.Contains(docId))
                    {
                        comparisonScore++;
                    }
                }

                metricsCalculator.AppendReduced(comparisonScore, searchVector, docId, reducedTargetVector);
            }
        }
    }
}
