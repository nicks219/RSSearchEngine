using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using RsseEngine.Contracts;
using RsseEngine.Dto;
using RsseEngine.Indexes;
using RsseEngine.Pools;

namespace RsseEngine.Algorithms;

/// <summary>
/// Класс с алгоритмом подсчёта сокращенной метрики.
/// Метрика считается GIN индексе, применены дополнительные оптимизации.
/// </summary>
public sealed class ReducedSearchGinFast : IReducedSearchProcessor
{
    public required TempStoragePool TempStoragePool { private get; init; }

    /// <summary>
    /// Общий индекс: идентификатор-вектор.
    /// </summary>
    public required DirectIndex GeneralDirectIndex { private get; init; }

    /// <summary>
    /// Общий инвертированный индекс: токен-идентификаторы.
    /// </summary>
    public required InvertedIndex<DocumentIdSet> GinReduced { private get; init; }

    /// <inheritdoc/>
    public void FindReduced(TokenVector searchVector, IMetricsCalculator metricsCalculator, CancellationToken cancellationToken)
    {
        // убираем дубликаты слов для intersect - это меняет результаты поиска (тексты типа "казино казино казино")
        searchVector = searchVector.DistinctAndGet();

        var idsFromGin = new List<DocumentIdSet>();

        foreach (var token in searchVector)
        {
            if (GinReduced.TryGetNonEmptyDocumentIdVector(token, out var ids))
            {
                idsFromGin.Add(ids);
            }
        }

        switch (idsFromGin.Count)
        {
            case 0:
                {
                    break;
                }
            case 1:
                {
                    foreach (var docId in idsFromGin[0])
                    {
                        metricsCalculator.AppendReduced(1, searchVector, docId, GeneralDirectIndex);
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

                            metricsCalculator.AppendReduced(comparisonScore, searchVector, docId,
                                GeneralDirectIndex);
                        }

                        if (cancellationToken.IsCancellationRequested)
                            throw new OperationCanceledException(nameof(ReducedSearchGinFast));

                        // Поиск в векторе reduced без учета самого тяжелого токена.
                        foreach (var (docId, comparisonScore) in comparisonScoresReduced)
                        {
                            metricsCalculator.AppendReduced(comparisonScore, searchVector, docId,
                                GeneralDirectIndex);
                        }
                    }
                    finally
                    {
                        TempStoragePool.ScoresTempStorage.Return(comparisonScoresReduced);
                    }

                    break;
                }
        }
    }
}
