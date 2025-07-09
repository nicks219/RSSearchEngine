using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using RsseEngine.Contracts;
using RsseEngine.Dto;
using RsseEngine.Indexes;
using RsseEngine.Pools;
using RsseEngine.Processor;

namespace RsseEngine.Algorithms;

/// <summary>
/// Класс с алгоритмом подсчёта сокращенной метрики.
/// Метрика считается GIN индексе, применены дополнительные оптимизации.
/// </summary>
public sealed class ReducedSearchGinFastFilter<TDocumentIdCollection> : IReducedSearchProcessor
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

    public required GinRelevanceFilter RelevanceFilter { private get; init; }

    /// <inheritdoc/>
    public void FindReduced(TokenVector searchVector, IMetricsCalculator metricsCalculator,
        CancellationToken cancellationToken)
    {
        // убираем дубликаты слов для intersect - это меняет результаты поиска (тексты типа "казино казино казино")
        searchVector = searchVector.DistinctAndGet();

        var comparisonScores = TempStoragePool.ScoresStorage.Get();
        var idsFromGin = TempStoragePool.GetDocumentIdCollectionList<TDocumentIdCollection>();

        try
        {
            if (!RelevanceFilter.FindFilteredDocumentsReduced(GinReduced, searchVector, comparisonScores, idsFromGin))
            {
                return;
            }

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
                        // сразу посчитаем на GIN метрики intersect.count для актуальных идентификаторов

                        var lastIndex = idsFromGin.Count - 1;

                        for (var index = 0; index < lastIndex; index++)
                        {
                            TDocumentIdCollection documentIdSet = idsFromGin[index];

                            if (comparisonScores.Count <= documentIdSet.Count)
                            {
                                foreach (var (documentId, _) in comparisonScores)
                                {
                                    if (!documentIdSet.Contains(documentId))
                                    {
                                        continue;
                                    }

                                    ref var score = ref CollectionsMarshal.GetValueRefOrNullRef(comparisonScores,
                                        documentId);

                                    if (!Unsafe.IsNullRef(ref score))
                                    {
                                        ++score;
                                    }
                                }
                            }
                            else
                            {
                                foreach (var documentId in documentIdSet)
                                {
                                    ref var score = ref CollectionsMarshal.GetValueRefOrNullRef(comparisonScores,
                                        documentId);

                                    if (!Unsafe.IsNullRef(ref score))
                                    {
                                        ++score;
                                    }
                                }
                            }
                        }

                        // Отдаём метрику на самый тяжелый токен поискового запроса.
                        {
                            TDocumentIdCollection documentIdSet = idsFromGin[lastIndex];

                            if (comparisonScores.Count <= documentIdSet.Count)
                            {
                                var removeSet = TempStoragePool.DocumentIdSetsStorage.Get();

                                try
                                {
                                    foreach (var (documentId, _) in comparisonScores)
                                    {
                                        if (!documentIdSet.Contains(documentId))
                                        {
                                            continue;
                                        }

                                        removeSet.Add(documentId);
                                    }

                                    foreach (var documentId in removeSet)
                                    {
                                        comparisonScores.Remove(documentId, out var comparisonScore);
                                        ++comparisonScore;

                                        metricsCalculator.AppendReduced(comparisonScore, searchVector, documentId,
                                            GeneralDirectIndex);
                                    }
                                }
                                finally
                                {
                                    TempStoragePool.DocumentIdSetsStorage.Return(removeSet);
                                }
                            }
                            else
                            {
                                foreach (var documentId in documentIdSet)
                                {
                                    if (!comparisonScores.ContainsKey(documentId))
                                    {
                                        continue;
                                    }

                                    comparisonScores.Remove(documentId, out var comparisonScore);
                                    ++comparisonScore;

                                    metricsCalculator.AppendReduced(comparisonScore, searchVector, documentId,
                                        GeneralDirectIndex);
                                }
                            }
                        }

                        if (cancellationToken.IsCancellationRequested)
                            throw new OperationCanceledException(nameof(ReducedSearchGinFast<TDocumentIdCollection>));

                        // Поиск в векторе reduced без учета самого тяжелого токена.
                        foreach (var (documentId, comparisonScore) in comparisonScores)
                        {
                            metricsCalculator.AppendReduced(comparisonScore, searchVector, documentId,
                                GeneralDirectIndex);
                        }

                        break;
                    }
            }
        }
        finally
        {
            TempStoragePool.ReturnDocumentIdCollectionList(idsFromGin);
            TempStoragePool.ScoresStorage.Return(comparisonScores);
        }
    }
}
