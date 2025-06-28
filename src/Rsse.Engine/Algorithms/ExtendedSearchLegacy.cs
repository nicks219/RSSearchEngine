using System;
using System.Threading;
using SearchEngine.Contracts;
using SearchEngine.Dto;
using SearchEngine.Indexes;
using SearchEngine.Processor;

namespace SearchEngine.Algorithms;

/// <summary>
/// Класс с "оригинальным" алгоритмом подсчёта расширенной метрики.
/// </summary>
public sealed class ExtendedSearchLegacy : IExtendedSearchProcessor
{
    /// <summary>
    /// Индекс для всех токенизированных заметок.
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
