using System;
using System.Collections.Generic;
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
public sealed class ReducedSearchGinFast<TDocumentIdCollection> : IReducedSearchProcessor
    where TDocumentIdCollection : struct, IDocumentIdCollection<TDocumentIdCollection>
{
    public required TempStoragePool TempStoragePool { private get; init; }

    /// <summary>
    /// Общий индекс: идентификатор-вектор.
    /// </summary>
    public required DirectIndex GeneralDirectIndex { private get; init; }

    /// <summary>
    /// Общий инвертированный индекс: токен-идентификаторы.
    /// </summary>
    public required InvertedIndex<TDocumentIdCollection> GinReduced { private get; init; }

    /// <inheritdoc/>
    public void FindReduced(TokenVector searchVector, IMetricsCalculator metricsCalculator, CancellationToken cancellationToken)
    {
        // убираем дубликаты слов для intersect - это меняет результаты поиска (тексты типа "казино казино казино")
        searchVector = searchVector.DistinctAndGet();

        List<TDocumentIdCollection> idsFromGin = TempStoragePool.GetDocumentIdCollectionList<TDocumentIdCollection>();

        try
        {
            GinReduced.GetNonEmptyDocumentIdVectorsToList(searchVector, idsFromGin);

            switch (idsFromGin.Count)
            {
                case 0:
                    {
                        break;
                    }
                case 1:
                    {
                        foreach (var documentId in idsFromGin[0])
                        {
                            metricsCalculator.AppendReduced(1, searchVector, documentId, GeneralDirectIndex);
                        }

                        break;
                    }
                default:
                    {
                        idsFromGin.Sort((left, right) => left.Count.CompareTo(right.Count));

                        // сразу посчитаем на GIN метрики intersect.count для актуальных идентификаторов
                        var comparisonScores = TempStoragePool.ScoresStorage.Get();

                        try
                        {
                            var lastIndex = idsFromGin.Count - 1;

                            for (var index = 0; index < lastIndex; index++)
                            {
                                foreach (var documentId in idsFromGin[index])
                                {
                                    ref var score = ref CollectionsMarshal.GetValueRefOrAddDefault(comparisonScores,
                                        documentId, out _);

                                    ++score;
                                }
                            }

                            // Отдаём метрику на самый тяжелый токен поискового запроса.
                            foreach (var documentId in idsFromGin[lastIndex])
                            {
                                comparisonScores.Remove(documentId, out var comparisonScore);
                                ++comparisonScore;

                                metricsCalculator.AppendReduced(comparisonScore, searchVector, documentId,
                                    GeneralDirectIndex);
                            }

                            if (cancellationToken.IsCancellationRequested)
                                throw new OperationCanceledException(nameof(ReducedSearchGinFast<TDocumentIdCollection>));

                            // Поиск в векторе reduced без учета самого тяжелого токена.
                            foreach (var (documentId, comparisonScore) in comparisonScores)
                            {
                                metricsCalculator.AppendReduced(comparisonScore, searchVector, documentId,
                                    GeneralDirectIndex);
                            }
                        }
                        finally
                        {
                            TempStoragePool.ScoresStorage.Return(comparisonScores);
                        }

                        break;
                    }
            }
        }
        finally
        {
             TempStoragePool.ReturnDocumentIdCollectionList(idsFromGin);
        }
    }
}
