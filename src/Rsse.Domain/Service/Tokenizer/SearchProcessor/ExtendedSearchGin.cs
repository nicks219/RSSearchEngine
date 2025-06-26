using System;
using System.Threading;
using Rsse.Search.Dto;
using Rsse.Search.Indexes;
using Rsse.Search.Processor;
using SearchEngine.Service.Tokenizer.Contracts;

namespace SearchEngine.Service.Tokenizer.SearchProcessor;

/// <summary>
/// Класс с алгоритмом подсчёта расширенной метрики.
/// Пространство поиска сокращается с помощью выбора из GIN индекса.
/// </summary>
public sealed class ExtendedSearchGin : IExtendedSearchProcessor
{
    /// <summary>
    /// Индекс для всех токенизированных заметок.
    /// </summary>
    public required DirectIndex GeneralDirectIndex { get; init; }

    /// <summary>
    /// Поддержка GIN-индекса.
    /// </summary>
    public required GinHandler<DocumentIdSet> GinExtended { get; init; }

    /// <inheritdoc/>
    public void FindExtended(TokenVector searchVector, MetricsCalculator metricsCalculator, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested) throw new OperationCanceledException(nameof(ExtendedSearchGin));

        // поиск в векторе extended
        foreach (var (docId, tokenLine) in GeneralDirectIndex)
        {
            if (!GinExtended.ContainsAnyTokenForDoc(searchVector, docId))
            {
                continue;
            }

            var extendedTargetVector = tokenLine.Extended;
            var comparisonScore = ScoreCalculator.ComputeOrdered(extendedTargetVector, searchVector);

            metricsCalculator.AppendExtended(comparisonScore, searchVector, docId, extendedTargetVector);
        }
    }
}
