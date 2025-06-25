using System;
using System.Threading;
using Rsse.Search.Dto;
using Rsse.Search.Indexes;

namespace SearchEngine.Service.Tokenizer.SearchProcessor;

/// <summary>
/// Класс с алгоритмом подсчёта сокращенной метрики.
/// Метрика считается с помощью GIN индекса в процессе поиска.
/// </summary>
public sealed class ReducedSearchGin : ReducedSearchProcessorBase
{
    /// <summary>
    /// Поддержка GIN-индекса.
    /// </summary>
    public required GinHandler<DocumentIdSet> GinReduced { get; init; }

    protected override void FindReduced(TokenVector reducedSearchVector, MetricsCalculator metricsCalculator, CancellationToken cancellationToken)
    {
        // убираем дубликаты слов для intersect - это меняет результаты поиска (тексты типа "казино казино казино")
        reducedSearchVector = reducedSearchVector.DistinctAndGet();

        // поиск в векторе reduced
        if (cancellationToken.IsCancellationRequested) throw new OperationCanceledException(nameof(ReducedSearchGin));
        foreach (var (docId, tokenLine) in GeneralDirectIndex)
        {
            var reducedTargetVector = tokenLine.Reduced;
            var comparisonScore = 0;
            foreach (var token in reducedSearchVector)
            {
                if (GinReduced.TryGetIdentifiers(token, out var reducedTokens) && reducedTokens.Contains(docId))
                {
                    comparisonScore++;
                }
            }

            metricsCalculator.AppendReduced(comparisonScore, reducedSearchVector, docId, reducedTargetVector);
        }
    }
}
