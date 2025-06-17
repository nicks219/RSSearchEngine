using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using SearchEngine.Service.Tokenizer.Contracts;
using SearchEngine.Service.Tokenizer.Dto;
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
        var docTokenMatchCounts = new Dictionary<DocId, int>();
        var pendingDocTokenCounts = new Dictionary<DocId, int>();
        var maxScore = reducedSearchVector.Count;
        var threshold = (int)(maxScore * MetricsCalculator.ReducedCoefficient) + 1;

        foreach (var token in reducedSearchVector)
        {
            if (!GinReduced.TryGetIdentifiers(token, out var ids))
            {
                continue;
            }

            foreach (var docId in ids)
            {
                // ref var score = ref CollectionsMarshal.GetValueRefOrAddDefault(docTokenMatchCounts, docId, out _);
                // ++score;

                // Можно сделать, чтобы 'tempScores' хранил только невостребованные значения, но условие получится сложное.
                ref var score = ref CollectionsMarshal.GetValueRefOrAddDefault(pendingDocTokenCounts, docId, out _);
                score++;

                if (score == threshold)
                {
                    // До определенного порога очки всё равно будут отброшены при расчете метрики в 'AppendReduced'.
                    docTokenMatchCounts[docId] = score;
                    continue;
                }

                if (score == maxScore)
                {
                    // Максимальный результат вносим сразу в метрику, минуя 'docTokenMatchCounts'.
                    metricsCalculator.AppendReduced(score, reducedSearchVector, docId, GeneralDirectIndex[docId].Reduced.Count);
                    continue;
                }

                if (score > threshold)
                {
                    // Сохраняем результат в диапазоне [threshold, maxScore).
                    docTokenMatchCounts[docId]++;
                }
            }
        }

        // поиск в векторе reduced
        if (cancellationToken.IsCancellationRequested) throw new OperationCanceledException(nameof(ReducedSearchGinOptimized));
        foreach (var (docId, comparisonScore) in docTokenMatchCounts)
        {
            // Вместо этого условия будем собирать в словарь только необходимые данные.
            // if (comparisonScore < threshold)
            // {
            //    continue;
            // }

            // выглядит так, что direct reduced-вектор в этом алгоритме вообще не нужен, нужен только его count.
            var reducedTargetVectorCount = GeneralDirectIndex[docId].Reduced.Count;

            metricsCalculator.AppendReduced(comparisonScore, reducedSearchVector, docId, reducedTargetVectorCount);
        }
    }
}

// --- --- ---
/*if (docTokenMatchCounts.TryGetValue(docId, out var existing))
{
    docTokenMatchCounts[docId] = existing + 1;
}
else if (pendingDocTokenCounts.TryGetValue(docId, out var temp))
{
    temp++;
    if (temp == threshold)
    {
        pendingDocTokenCounts.Remove(docId);
        docTokenMatchCounts[docId] = threshold;
    }
    else
    {
        pendingDocTokenCounts[docId] = temp;
    }
}
else
{
    pendingDocTokenCounts[docId] = 1;
}*/
