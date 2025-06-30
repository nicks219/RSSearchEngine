using System;
using System.Threading;
using RsseEngine.Contracts;
using RsseEngine.Dto;
using RsseEngine.Indexes;
using RsseEngine.Processor;

namespace RsseEngine.Algorithms;

/// <summary>
/// Класс с "оригинальным" алгоритмом подсчёта расширенной метрики.
/// </summary>
public sealed class ExtendedSearchLegacy : IExtendedSearchProcessor
{
    /// <summary>
    /// Общий индекс: идентификатор-вектор.
    /// </summary>
    public required DirectIndex GeneralDirectIndex { get; init; }

    /// <inheritdoc/>
    public void FindExtended(TokenVector searchVector, IMetricsCalculator metricsCalculator, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested) throw new OperationCanceledException(nameof(ExtendedSearchLegacy));

        // поиск в векторе extended
        foreach (var (docId, tokenLine) in GeneralDirectIndex)
        {
            var extendedTargetVector = tokenLine.Extended;
            var comparisonScore = ScoreCalculator.ComputeOrdered(extendedTargetVector, searchVector);

            metricsCalculator.AppendExtended(comparisonScore, searchVector, docId, extendedTargetVector);
        }
    }
}
