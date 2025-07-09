using System;
using System.Runtime.InteropServices;
using System.Threading;
using RsseEngine.Contracts;
using RsseEngine.Dto;
using RsseEngine.Indexes;
using RsseEngine.Pools;

namespace RsseEngine.Algorithms;

/// <summary>
/// Класс с алгоритмом подсчёта сокращенной метрики.
/// Метрика считается на самом GIN индексе и создаётся промежуточный результат для поиска в нём.
/// </summary>
public sealed class ReducedSearchGinOptimized<TDocumentIdCollection> : IReducedSearchProcessor
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
    public void FindReduced(TokenVector searchVector, IMetricsCalculator metricsCalculator,
        CancellationToken cancellationToken)
    {
        // убираем дубликаты слов для intersect - это меняет результаты поиска (тексты типа "казино казино казино")
        searchVector = searchVector.DistinctAndGet();

        var comparisonScores = TempStoragePool.ScoresStorage.Get();

        try
        {
            // сразу посчитаем на GIN метрики intersect.count для актуальных идентификаторов
            foreach (var token in searchVector)
            {
                if (!GinReduced.TryGetNonEmptyDocumentIdVector(token, out var documentIds))
                {
                    continue;
                }

                foreach (var documentId in documentIds)
                {
                    ref var score = ref CollectionsMarshal.GetValueRefOrAddDefault(comparisonScores, documentId, out _);
                    ++score;
                }
            }

            if (cancellationToken.IsCancellationRequested)
                throw new OperationCanceledException(nameof(ReducedSearchGinOptimized<TDocumentIdCollection>));

            // поиск в векторе reduced
            foreach (var (documentId, comparisonScore) in comparisonScores)
            {
                metricsCalculator.AppendReduced(comparisonScore, searchVector, documentId, GeneralDirectIndex);
            }
        }
        finally
        {
            TempStoragePool.ScoresStorage.Return(comparisonScores);
        }
    }
}
