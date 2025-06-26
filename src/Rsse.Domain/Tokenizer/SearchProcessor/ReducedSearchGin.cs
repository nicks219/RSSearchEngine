using System;
using System.Threading;
using SearchEngine.Tokenizer.Contracts;
using SearchEngine.Tokenizer.Indexes;
using SearchEngine.Tokenizer.TokenizerProcessor;

namespace SearchEngine.Tokenizer.SearchProcessor;

/// <summary>
/// Класс с алгоритмом подсчёта сокращенной метрики.
/// Метрика считается с помощью GIN индекса в процессе поиска.
/// </summary>
public sealed class ReducedSearchGin : ReducedSearchProcessorBase, IReducedSearchProcessor
{
    /// <summary>
    /// Поддержка GIN-индекса.
    /// </summary>
    public required InvertedIndexHandler InvertedIndexReduced { get; init; }

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
        if (cancellationToken.IsCancellationRequested) throw new OperationCanceledException(nameof(ReducedSearchGin));
        var directIndex = DirectIndexHandler.GetGeneralDirectIndex;
        foreach (var (docId, tokenLine) in directIndex)
        {
            var comparisonScore = 0;
            foreach (var token in reducedSearchVector)
            {
                if (InvertedIndexReduced.TryGetIdentifiers(token, out var reducedTokens) && reducedTokens.Contains(docId))
                {
                    comparisonScore++;
                }
            }

            var reducedTargetVector = tokenLine.Reduced;
            metricsCalculator.AppendReduced(comparisonScore, reducedSearchVector, docId, reducedTargetVector);
        }
    }
}
