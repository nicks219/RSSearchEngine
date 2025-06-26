using System;
using System.Threading;
using Rsse.Search.Dto;
using Rsse.Search.Indexes;
using SearchEngine.Service.Tokenizer.Contracts;

namespace SearchEngine.Service.Tokenizer.SearchProcessor;

/// <summary>
/// Класс с алгоритмом подсчёта сокращенной метрики.
/// Метрика считается с помощью GIN индекса в процессе поиска.
/// </summary>
public sealed class ReducedSearchGin : IReducedSearchProcessor
{
    /// <summary>
    /// Индекс для всех токенизированных заметок.
    /// </summary>
    public required DirectIndex GeneralDirectIndex { get; init; }

    /// <summary>
    /// Поддержка GIN-индекса.
    /// </summary>
    public required GinHandler<DocumentIdSet> GinReduced { get; init; }

    /// <inheritdoc/>
    public void FindReduced(TokenVector searchVector, MetricsCalculator metricsCalculator, CancellationToken cancellationToken)
    {
        // убираем дубликаты слов для intersect - это меняет результаты поиска (тексты типа "казино казино казино")
        searchVector = searchVector.DistinctAndGet();

        if (cancellationToken.IsCancellationRequested) throw new OperationCanceledException(nameof(ReducedSearchGin));

        // поиск в векторе reduced
        foreach (var (docId, tokenLine) in GeneralDirectIndex)
        {
            var reducedTargetVector = tokenLine.Reduced;
            var comparisonScore = 0;
            foreach (var token in searchVector)
            {
                if (GinReduced.TryGetIdentifiers(token, out var reducedTokens) && reducedTokens.Contains(docId))
                {
                    comparisonScore++;
                }
            }

            metricsCalculator.AppendReduced(comparisonScore, searchVector, docId, reducedTargetVector);
        }
    }
}
