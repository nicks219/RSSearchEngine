using System;
using System.Threading;
using SearchEngine.Contracts;
using SearchEngine.Dto;
using SearchEngine.Indexes;
using SearchEngine.Processor;

namespace SearchEngine.Algorithms;

/// <summary>
/// Класс с "оригинальным" алгоритмом подсчёта сокращенной метрики.
/// </summary>
public sealed class ReducedSearchLegacy : IReducedSearchProcessor
{
    /// <summary>
    /// Индекс для всех токенизированных заметок.
    /// </summary>
    public required DirectIndex GeneralDirectIndex { get; init; }

    /// <inheritdoc/>
    public void FindReduced(TokenVector searchVector, IMetricsCalculator metricsCalculator, CancellationToken cancellationToken)
    {
        // убираем дубликаты слов для intersect - это меняет результаты поиска (тексты типа "казино казино казино")
        searchVector = searchVector.DistinctAndGet();

        if (cancellationToken.IsCancellationRequested) throw new OperationCanceledException(nameof(ReducedSearchLegacy));

        // поиск в векторе reduced
        foreach (var (docId, tokenLine) in GeneralDirectIndex)
        {
            var reducedTargetVector = tokenLine.Reduced;
            var comparisonScore = ScoreCalculator.ComputeUnordered(reducedTargetVector, searchVector);

            metricsCalculator.AppendReduced(comparisonScore, searchVector, docId, reducedTargetVector);
        }
    }
}
