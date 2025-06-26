using System;
using System.Threading;
using Rsse.Search.Dto;
using Rsse.Search.Indexes;
using Rsse.Search.Processor;
using SearchEngine.Service.Tokenizer.Contracts;

namespace SearchEngine.Service.Tokenizer.SearchProcessor;

/// <summary>
/// Класс с "оригинальным" алгоритмом подсчёта сокращенной метрики.
/// </summary>
public sealed class ReducedSearch : IReducedSearchProcessor
{
    /// <summary>
    /// Индекс для всех токенизированных заметок.
    /// </summary>
    public required DirectIndex GeneralDirectIndex { get; init; }

    /// <inheritdoc/>
    public void FindReduced(TokenVector searchVector, MetricsCalculator metricsCalculator, CancellationToken cancellationToken)
    {
        // убираем дубликаты слов для intersect - это меняет результаты поиска (тексты типа "казино казино казино")
        searchVector = searchVector.DistinctAndGet();

        if (cancellationToken.IsCancellationRequested) throw new OperationCanceledException(nameof(ReducedSearch));

        // поиск в векторе reduced
        foreach (var (docId, tokenLine) in GeneralDirectIndex)
        {
            var reducedTargetVector = tokenLine.Reduced;
            var comparisonScore = ScoreCalculator.ComputeUnordered(reducedTargetVector, searchVector);

            metricsCalculator.AppendReduced(comparisonScore, searchVector, docId, reducedTargetVector);
        }
    }
}
