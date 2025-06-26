using System;
using System.Threading;
using SearchEngine.Tokenizer.Contracts;
using SearchEngine.Tokenizer.TokenizerProcessor;

namespace SearchEngine.Tokenizer.SearchProcessor;

/// <summary>
/// Класс с "оригинальным" алгоритмом подсчёта сокращенной метрики.
/// </summary>
public sealed class ReducedSearch : ReducedSearchProcessorBase, IReducedSearchProcessor
{
    /// <inheritdoc/>
    public void FindReduced(string text, MetricsCalculator metricsCalculator, CancellationToken cancellationToken)
    {
        var processor = TokenizerProcessorFactory.CreateProcessor(ProcessorType.Reduced);

        var reducedSearchVector = processor.TokenizeText(text);

        if (reducedSearchVector.Count == 0)
        {
            // песни вида "123 456" не ищем, так как получим весь каталог
            return;
        }

        // убираем дубликаты слов для intersect - это меняет результаты поиска (тексты типа "казино казино казино")
        reducedSearchVector = reducedSearchVector.DistinctAndGet();

        // поиск в векторе reduced
        if (cancellationToken.IsCancellationRequested) throw new OperationCanceledException(nameof(ReducedSearch));
        var directIndex = DirectIndexHandler.GetGeneralDirectIndex;
        foreach (var (docId, tokenLine) in directIndex)
        {
            var reducedTargetVector = tokenLine.Reduced;
            var comparisonScore = processor.ComputeComparisonScore(reducedTargetVector, reducedSearchVector);

            metricsCalculator.AppendReduced(comparisonScore, reducedSearchVector, docId, reducedTargetVector);
        }
    }
}
