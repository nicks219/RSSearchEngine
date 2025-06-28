using System;
using System.Threading;
using RsseEngine.Contracts;
using RsseEngine.Dto;
using RsseEngine.Indexes;
using RsseEngine.Processor;

namespace RsseEngine.Algorithms;

/// <summary>
/// Класс с "оригинальным" алгоритмом подсчёта сокращенной метрики.
/// </summary>
public sealed class ReducedSearchLegacy : IReducedSearchProcessor
{
    /// <summary>
    /// Общий индекс: идентификатор-вектор.
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
