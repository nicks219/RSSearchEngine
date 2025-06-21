using System;
using System.Collections.Generic;
using System.Linq;
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

        var idsFromGin = new List<DocIdVector>();
        foreach (var token in reducedSearchVector)
        {
            if (GinReduced.TryGetIdentifiers(token, out var ids) && ids.Count > 0)
            {
                idsFromGin.Add(ids);
            }
        }

        switch (idsFromGin.Count)
        {
            case 0: break;

            case 1:
            {
                foreach (var docId in idsFromGin[0])
                {
                    metricsCalculator.AppendReduced(1, reducedSearchVector, docId, GeneralDirectIndex);
                }

                break;
            }

            default:
            {
                idsFromGin = idsFromGin.OrderBy(docIdVector => docIdVector.Count).ToList();

                // сразу посчитаем на GIN метрики intersect.count для актуальных идентификаторов
                var comparisonScoresReduced = TempStoragePool.ScoresTempStorage.Get();

                try
                {
                    var lastIndex = idsFromGin.Count - 1;

                    for (var index = 0; index < lastIndex; index++)
                    {
                        foreach (var docId in idsFromGin[index])
                        {
                            ref var score = ref CollectionsMarshal.GetValueRefOrAddDefault(comparisonScoresReduced,
                                docId, out _);

                            ++score;
                        }
                    }

                    // Отдаём метрику на самый тяжелый токен поискового запроса.
                    foreach (var docId in idsFromGin[lastIndex])
                    {
                        comparisonScoresReduced.Remove(docId, out var comparisonScore);
                        ++comparisonScore;

                        metricsCalculator.AppendReduced(comparisonScore, reducedSearchVector, docId,
                            GeneralDirectIndex);
                    }

                    if (cancellationToken.IsCancellationRequested)
                        throw new OperationCanceledException(nameof(ReducedSearchGinFast));

                    // Поиск в векторе reduced без учета самого тяжелого токена.
                    foreach (var (docId, comparisonScore) in comparisonScoresReduced)
                    {
                        metricsCalculator.AppendReduced(comparisonScore, reducedSearchVector, docId,
                            GeneralDirectIndex);
                    }
                }
                finally
                {
                    comparisonScoresReduced.Clear();
                    TempStoragePool.ScoresTempStorage.Return(comparisonScoresReduced);
                }

                break;
            }

        }
    }
}
