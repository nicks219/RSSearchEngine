using System;
using System.Runtime.InteropServices;
using System.Threading;
using SearchEngine.Service.Tokenizer.Contracts;
using SearchEngine.Service.Tokenizer.Indexes;
using SearchEngine.Service.Tokenizer.TokenizerProcessor;

namespace SearchEngine.Service.Tokenizer.SearchProcessor;

/// <summary>
/// Класс с алгоритмом подсчёта сокращенной метрики.
/// Метрика считается GIN индексе, применены дополнительные оптимизации.
/// </summary>
public sealed class ReducedSearchGinFast : ReducedSearchProcessorBase, IReducedSearchProcessor
{
    /// <summary>
    /// Поддержка GIN-индекса.
    /// </summary>
    public required GinHandler GinReduced { get; init; }

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

        // сразу посчитаем на GIN метрики intersect.count для актуальных идентификаторов
        var comparisonScoresReduced = TempStoragePool.ScoresTempStorage.Get();
        try
        {

            // comparisonScoresReduced.Clear();

            foreach (var token in reducedSearchVector)
            {
                if (!GinReduced.TryGetIdentifiers(token, out var ids))
                {
                    continue;
                }

                foreach (var docId in ids)
                {
                    ref var score =
                        ref CollectionsMarshal.GetValueRefOrAddDefault(comparisonScoresReduced, docId, out _);
                    ++score;
                }
            }

            // поиск в векторе reduced
            if (cancellationToken.IsCancellationRequested)
                throw new OperationCanceledException(nameof(ReducedSearchGinOptimized));
            foreach (var (docId, comparisonScore) in comparisonScoresReduced)
            {
                var reducedTargetVector = GeneralDirectIndex[docId].Reduced;

                metricsCalculator.AppendReduced(comparisonScore, reducedSearchVector, docId, reducedTargetVector);
            }
        }
        finally
        {
            // Чистим коллекцию перед возвращением в пул.
            comparisonScoresReduced.Clear();
            TempStoragePool.ScoresTempStorage.Return(comparisonScoresReduced);
        }
    }
}
