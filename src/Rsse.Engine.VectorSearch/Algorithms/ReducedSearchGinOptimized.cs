using System;
using System.Threading;
using RsseEngine.Contracts;
using RsseEngine.Dto;
using RsseEngine.Indexes;
using RsseEngine.Pools;
using RsseEngine.Processor;

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
        var comparisonScores = new ComparisonScores(TempStoragePool.ScoresStorage.Get());

        try
        {
            // сразу посчитаем на GIN метрики intersect.count для актуальных идентификаторов
            foreach (var token in searchVector)
            {
                if (!GinReduced.TryGetNonEmptyDocumentIdVector(token, out var documentIds))
                {
                    continue;
                }

                comparisonScores.AddAll(documentIds);
            }

            if (cancellationToken.IsCancellationRequested)
                throw new OperationCanceledException(nameof(ReducedSearchGinOptimized<TDocumentIdCollection>));

            // поиск в векторе reduced
            metricsCalculator.AppendReducedMetrics(GeneralDirectIndex, searchVector, comparisonScores);
        }
        finally
        {
            TempStoragePool.ScoresStorage.Return(comparisonScores.Dictionary);
        }
    }
}
