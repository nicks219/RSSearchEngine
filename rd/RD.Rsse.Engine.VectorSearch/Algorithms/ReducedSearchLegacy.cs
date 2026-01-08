using System;
using System.Threading;
using RD.RsseEngine.Contracts;
using RD.RsseEngine.Dto;
using RD.RsseEngine.Indexes;
using RD.RsseEngine.Processor;

namespace RD.RsseEngine.Algorithms;

/// <summary>
/// Класс с "оригинальным" алгоритмом подсчёта сокращенной метрики.
/// </summary>
public readonly ref struct ReducedSearchLegacy : IReducedSearchProcessor
{
    /// <summary>
    /// Общий индекс: идентификатор-вектор.
    /// </summary>
    public required DirectIndex GeneralDirectIndex { private get; init; }

    /// <inheritdoc/>
    public void FindReduced(TokenVector searchVector, IMetricsCalculator metricsCalculator, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
            throw new OperationCanceledException(nameof(ReducedSearchLegacy));

        // поиск в векторе reduced
        foreach (var (documentId, tokenLine) in GeneralDirectIndex)
        {
            metricsCalculator.AppendReducedMetric(searchVector, documentId, tokenLine);
        }
    }
}
