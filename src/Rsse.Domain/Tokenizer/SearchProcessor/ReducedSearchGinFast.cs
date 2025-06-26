using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using SearchEngine.Tokenizer.Contracts;
using SearchEngine.Tokenizer.Dto;
using SearchEngine.Tokenizer.Indexes;
using SearchEngine.Tokenizer.TokenizerProcessor;

namespace SearchEngine.Tokenizer.SearchProcessor;

/// <summary>
/// Класс с алгоритмом подсчёта сокращенной метрики.
/// Метрика считается GIN индексе, применены дополнительные оптимизации.
/// </summary>
public sealed class ReducedSearchGinFast : ReducedSearchProcessorBase, IReducedSearchProcessor
{
    /// <summary>
    /// Поддержка GIN-индекса.
    /// </summary>
    public required InvertedIndexHandler InvertedIndexReduced { get; init; }

    /// <inheritdoc/>
    public void FindReduced(string text, MetricsCalculator metricsCalculator, CancellationToken cancellationToken)
    {
        var processor = TokenizerProcessorFactory.CreateProcessor(ProcessorType.Reduced);

        var searchVector = processor.TokenizeText(text);

        if (searchVector.Count == 0)
        {
            // заметки вида "123 456" не ищем, так как получим весь каталог
            return;
        }

        // убираем дубликаты слов для intersect - это меняет результаты поиска (тексты типа "казино казино казино")
        searchVector = searchVector.DistinctAndGet();

        var idsFromGin = new List<DocIdVector>();
        foreach (var token in searchVector)
        {
            if (InvertedIndexReduced.TryGetIdentifiers(token, out var ids) && ids.Count > 0)
            {
                idsFromGin.Add(ids);
            }
        }

        // direct reduced - нужен только count | к docid добавить размер в extended и reduced
        var directIndex = DirectIndexHandler.GetGeneralDirectIndex;
        var threshold = searchVector.Count * MetricsCalculator.ReducedCoefficient;
        switch (idsFromGin.Count)
        {
            case 0: return;

            case 1:
                {
                    foreach (var docId in idsFromGin[0])
                    {
                        if (1 >= threshold)
                        {
                            var targetVectorCount = directIndex[docId].Reduced.Count;
                            metricsCalculator.AppendReduced(1, searchVector, docId, targetVectorCount);
                        }
                    }

                    return;
                }

            default:
                {
                    idsFromGin = idsFromGin.OrderBy(docIdVector => docIdVector.Count).ToList();

                    // сразу посчитаем на GIN метрики intersect.count для актуальных идентификаторов
                    var comparisonScores = TempStoragePool.ScoresTempStorage.Get();

                    try
                    {
                        var lastIndex = idsFromGin.Count - 1;

                        for (var index = 0; index < lastIndex; index++)
                        {
                            foreach (var docId in idsFromGin[index])
                            {
                                ref var score = ref CollectionsMarshal.GetValueRefOrAddDefault(comparisonScores,
                                    docId, out _);

                                ++score;
                            }
                        }

                        // Отдаём метрику на самый тяжелый токен поискового запроса.
                        foreach (var docId in idsFromGin[lastIndex])
                        {
                            comparisonScores.Remove(docId, out var comparisonScore);
                            ++comparisonScore;

                            if (comparisonScore >= threshold)
                            {
                                var targetVectorCount = directIndex[docId].Reduced.Count;
                                metricsCalculator.AppendReduced(comparisonScore, searchVector, docId, targetVectorCount);
                            }
                        }

                        if (cancellationToken.IsCancellationRequested)
                            throw new OperationCanceledException(nameof(ReducedSearchGinFast));

                        // Поиск в векторе reduced без учета самого тяжелого токена.
                        foreach (var (docId, comparisonScore) in comparisonScores)
                        {
                            if (comparisonScore >= threshold)
                            {
                                var targetVectorCount = directIndex[docId].Reduced.Count;
                                metricsCalculator.AppendReduced(comparisonScore, searchVector, docId, targetVectorCount);
                            }
                        }
                    }
                    finally
                    {
                        comparisonScores.Clear();
                        TempStoragePool.ScoresTempStorage.Return(comparisonScores);
                    }

                    return;
                }
        }
    }
}
