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
public sealed class ExtendedSearchGinFilter : IExtendedSearchProcessor
{
    public required TempStoragePool TempStoragePool { private get; init; }

    /// <summary>
    /// Общий индекс: идентификатор-вектор.
    /// </summary>
    public required DirectIndex GeneralDirectIndex { private get; init; }

    /// <summary>
    /// Общий инвертированный индекс: токен-идентификаторы.
    /// </summary>
    public required InvertedIndex<DocumentIdSet> GinExtended { private get; init; }

    public required GinRelevanceFilter RelevanceFilter { private get; init; }

    /// <inheritdoc/>
    public void FindExtended(TokenVector searchVector, IMetricsCalculator metricsCalculator,
        CancellationToken cancellationToken)
    {
        var filteredDocuments = TempStoragePool.SetsTempStorage.Get();
        var idsFromGin = TempStoragePool.DocumentIdSetListsTempStorage.Get();

        try
        {
            if (!RelevanceFilter.ProcessToSet(GinExtended, searchVector, filteredDocuments, idsFromGin))
            {
                return;
            }

            if (cancellationToken.IsCancellationRequested)
                throw new OperationCanceledException(nameof(ExtendedSearchGinFilter));

            // поиск в векторе extended
            foreach (var documentId in filteredDocuments)
            {
                var extendedTargetVector = GeneralDirectIndex[documentId].Extended;
                var comparisonScore = ScoreCalculator.ComputeOrdered(extendedTargetVector, searchVector);

                // Для расчета метрик необходимо учитывать размер оригинальной заметки.
                metricsCalculator.AppendExtended(comparisonScore, searchVector, documentId, extendedTargetVector);
            }
        }
        finally
        {
            TempStoragePool.DocumentIdSetListsTempStorage.Return(idsFromGin);
            TempStoragePool.SetsTempStorage.Return(filteredDocuments);
        }
    }
}
