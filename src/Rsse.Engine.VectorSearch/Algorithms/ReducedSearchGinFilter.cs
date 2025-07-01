using System;
using System.Threading;
using RsseEngine.Contracts;
using RsseEngine.Dto;
using RsseEngine.Indexes;
using RsseEngine.Pools;
using RsseEngine.Processor;

namespace RsseEngine.Algorithms;

/// <summary>
/// Класс с алгоритмом подсчёта расширенной метрики.
/// Пространство поиска формируется с помощью GIN индекса.
/// </summary>
public sealed class ReducedSearchGinFilter : IReducedSearchProcessor
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

    public required GinRelevanceFilter RelevanceFilter { private get; init; }

    /// <inheritdoc/>
    public void FindReduced(TokenVector searchVector, IMetricsCalculator metricsCalculator, CancellationToken cancellationToken)
    {
        // убираем дубликаты слов для intersect - это меняет результаты поиска (тексты типа "казино казино казино")
        searchVector = searchVector.DistinctAndGet();

        var filteredDocuments = RelevanceFilter.ProcessToSet(GinReduced, searchVector);
        if (filteredDocuments.Count == 0)
        {
            return;
        }

        if (cancellationToken.IsCancellationRequested) throw new OperationCanceledException(nameof(ReducedSearchGinFilter));

        // поиск в векторе reduced
        foreach (var documentId in filteredDocuments)
        {
            var reducedTargetVector = GeneralDirectIndex[documentId].Reduced;
            var comparisonScore = ScoreCalculator.ComputeUnordered(reducedTargetVector, searchVector);

            // Для расчета метрик необходимо учитывать размер оригинальной заметки.
            metricsCalculator.AppendReduced(comparisonScore, searchVector, documentId, reducedTargetVector);
        }
    }
}
