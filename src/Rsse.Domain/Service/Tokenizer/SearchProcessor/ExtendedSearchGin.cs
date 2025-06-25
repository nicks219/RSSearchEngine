using System;
using System.Threading;
using Rsse.Search.Dto;
using Rsse.Search.Indexes;
using Rsse.Search.Processor;

namespace SearchEngine.Service.Tokenizer.SearchProcessor;

/// <summary>
/// Класс с алгоритмом подсчёта расширенной метрики.
/// Пространство поиска сокращается с помощью выбора из GIN индекса.
/// </summary>
public sealed class ExtendedSearchGin : ExtendedSearchProcessorBase
{
    /// <summary>
    /// Поддержка GIN-индекса.
    /// </summary>
    public required GinHandler<DocumentIdSet> GinExtended { get; init; }

    protected override void FindExtended(TokenVector extendedSearchVector, MetricsCalculator metricsCalculator, CancellationToken cancellationToken)
    {
        // поиск в векторе extended
        if (cancellationToken.IsCancellationRequested) throw new OperationCanceledException(nameof(ExtendedSearchGin));
        foreach (var (docId, tokenLine) in GeneralDirectIndex)
        {
            if (!GinExtended.ContainsAnyTokenForDoc(extendedSearchVector, docId))
            {
                continue;
            }

            var extendedTargetVector = tokenLine.Extended;
            var comparisonScore = ScoreCalculator.ComputeOrdered(extendedTargetVector, extendedSearchVector);

            metricsCalculator.AppendExtended(comparisonScore, extendedSearchVector, docId, extendedTargetVector);
        }
    }
}
