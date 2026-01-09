using System;
using System.Threading;
using SimpleEngine.Contracts;
using SimpleEngine.Dto.Common;
using SimpleEngine.Indexes;
using SimpleEngine.Processor;

namespace SimpleEngine.Algorithms.Legacy;

/// <summary>
/// Класс с "оригинальным" алгоритмом подсчёта сокращенной метрики.
/// </summary>
public readonly ref struct ReducedSearchLegacy : IReducedSearchProcessor
{
    /// <summary>
    /// Общий индекс: идентификатор-вектор.
    /// </summary>
    public required GeneralDirectIndexLegacy GeneralDirectIndexLegacy { private get; init; }

    /// <inheritdoc/>
    public void FindReduced(TokenVector searchVector, IMetricsCalculator metricsCalculator, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            throw new OperationCanceledException(nameof(ReducedSearchLegacy));
        }

        // поиск в векторе reduced
        foreach (var (documentId, tokenLine) in GeneralDirectIndexLegacy)
        {
            metricsCalculator.AppendReducedMetric(searchVector, documentId, tokenLine);
        }
    }
}
