using System;
using System.Threading;
using Rsse.Search.Dto;
using Rsse.Search.Indexes;
using Rsse.Search.Processor;
using SearchEngine.Service.Tokenizer.Contracts;

namespace SearchEngine.Service.Tokenizer.SearchProcessor;

/// <summary>
/// Класс с "оригинальным" алгоритмом подсчёта расширенной метрики.
/// </summary>
public sealed class ExtendedSearch : IExtendedSearchProcessor
{
    /// <summary>
    /// Индекс для всех токенизированных заметок.
    /// </summary>
    public required DirectIndex GeneralDirectIndex { get; init; }

    /// <inheritdoc/>
    public void FindExtended(TokenVector searchVector, MetricsCalculator metricsCalculator, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested) throw new OperationCanceledException(nameof(ExtendedSearch));

        // поиск в векторе extended
        foreach (var (docId, tokenLine) in GeneralDirectIndex)
        {
            var extendedTargetVector = tokenLine.Extended;
            var comparisonScore = ScoreCalculator.ComputeOrdered(extendedTargetVector, searchVector);

            metricsCalculator.AppendExtended(comparisonScore, searchVector, docId, extendedTargetVector);
        }
    }
}
