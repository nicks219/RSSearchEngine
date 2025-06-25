using System;
using System.Threading;
using Rsse.Search.Dto;
using Rsse.Search.Processor;

namespace SearchEngine.Service.Tokenizer.SearchProcessor;

/// <summary>
/// Класс с "оригинальным" алгоритмом подсчёта сокращенной метрики.
/// </summary>
public sealed class ReducedSearch : ReducedSearchProcessorBase
{
    protected override void FindReduced(TokenVector reducedSearchVector, MetricsCalculator metricsCalculator, CancellationToken cancellationToken)
    {
        // убираем дубликаты слов для intersect - это меняет результаты поиска (тексты типа "казино казино казино")
        reducedSearchVector = reducedSearchVector.DistinctAndGet();

        // поиск в векторе reduced
        if (cancellationToken.IsCancellationRequested) throw new OperationCanceledException(nameof(ReducedSearch));
        foreach (var (docId, tokenLine) in GeneralDirectIndex)
        {
            var reducedTargetVector = tokenLine.Reduced;
            var comparisonScore = ScoreCalculator.ComputeUnordered(reducedTargetVector, reducedSearchVector);

            metricsCalculator.AppendReduced(comparisonScore, reducedSearchVector, docId, reducedTargetVector);
        }
    }
}
