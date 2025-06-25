using System;
using System.Threading;
using Rsse.Search.Dto;
using Rsse.Search.Processor;

namespace SearchEngine.Service.Tokenizer.SearchProcessor;

/// <summary>
/// Класс с "оригинальным" алгоритмом подсчёта расширенной метрики.
/// </summary>
public sealed class ExtendedSearch : ExtendedSearchProcessorBase
{
    protected override void FindExtended(TokenVector extendedSearchVector, MetricsCalculator metricsCalculator, CancellationToken cancellationToken)
    {
        // поиск в векторе extended
        if (cancellationToken.IsCancellationRequested) throw new OperationCanceledException(nameof(ExtendedSearch));
        foreach (var (docId, tokenLine) in GeneralDirectIndex)
        {
            var extendedTargetVector = tokenLine.Extended;
            var comparisonScore = ScoreCalculator.ComputeOrdered(extendedTargetVector, extendedSearchVector);

            metricsCalculator.AppendExtended(comparisonScore, extendedSearchVector, docId, extendedTargetVector);
        }
    }
}
