using System;
using System.Threading;
using RD.RsseEngine.Contracts;
using RD.RsseEngine.Dto;
using RD.RsseEngine.Indexes;
using RD.RsseEngine.Processor;

namespace RD.RsseEngine.Algorithms;

/// <summary>
/// Класс с "оригинальным" алгоритмом подсчёта расширенной метрики.
/// </summary>
public readonly ref struct ExtendedSearchLegacy : IExtendedSearchProcessor
{
    /// <summary>
    /// Общий индекс: идентификатор-вектор.
    /// </summary>
    public required DirectIndex GeneralDirectIndex { private get; init; }

    /// <inheritdoc/>
    public void FindExtended(TokenVector searchVector, IMetricsCalculator metricsCalculator,
        CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
            throw new OperationCanceledException(nameof(ExtendedSearchLegacy));

        // поиск в векторе extended
        foreach (var (documentId, tokenLine) in GeneralDirectIndex)
        {
            metricsCalculator.AppendExtendedMetric(searchVector, documentId, tokenLine);
        }
    }
}
