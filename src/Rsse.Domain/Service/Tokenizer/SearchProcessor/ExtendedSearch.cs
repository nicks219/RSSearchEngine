using System;
using System.Threading;
using SearchEngine.Service.Tokenizer.Contracts;
using SearchEngine.Service.Tokenizer.TokenizerProcessor;

namespace SearchEngine.Service.Tokenizer.SearchProcessor;

/// <summary>
/// Класс с "оригинальным" алгоритмом подсчёта расширенной метрики.
/// </summary>
public class ExtendedSearch : ExtendedSearchProcessorBase, IExtendedSearchProcessor
{
    /// <inheritdoc/>
    public bool FindExtended(string text, MetricsCalculator metricsCalculator, CancellationToken cancellationToken)
    {
        var processor = TokenizerProcessorFactory.CreateProcessor(ProcessorType.Extended);

        var extendedSearchVector = processor.TokenizeText(text);

        if (extendedSearchVector.Count == 0)
        {
            // заметки вида "123 456" не ищем, так как получим весь каталог
            return false;
        }

        // поиск в векторе extended
        if (cancellationToken.IsCancellationRequested) throw new OperationCanceledException(nameof(ExtendedSearch));
        foreach (var (docId, tokenLine) in GeneralDirectIndex)
        {
            var extendedTargetVector = tokenLine.Extended;
            var comparisonScore = processor.ComputeComparisonScore(extendedTargetVector, extendedSearchVector);

            metricsCalculator.AppendExtended(comparisonScore, extendedSearchVector, docId, extendedTargetVector);
        }

        return true;
    }
}
