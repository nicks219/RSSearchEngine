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
public sealed class ExtendedSearchGinOptimized<TDocumentIdCollection> : IExtendedSearchProcessor
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
    public required InvertedIndex<TDocumentIdCollection> GinExtended { private get; init; }

    /// <inheritdoc/>
    public void FindExtended(TokenVector searchVector, IMetricsCalculator metricsCalculator,
        CancellationToken cancellationToken)
    {
        var processedDocuments = TempStoragePool.DocumentIdSetsStorage.Get();
        var processedTokens = TempStoragePool.TokenSetsStorage.Get();

        try
        {
            // выбрать только те заметки, которые пригодны для extended поиска
            foreach (var token in searchVector)
            {
                if (!GinExtended.TryGetNonEmptyDocumentIdVector(token, out var documentIds))
                {
                    continue;
                }

                if (!processedTokens.Add(token))
                {
                    continue;
                }

                foreach (var documentId in documentIds)
                {
                    processedDocuments.Add(documentId);
                }
            }

            if (cancellationToken.IsCancellationRequested)
                throw new OperationCanceledException(nameof(ExtendedSearchGinOptimized<TDocumentIdCollection>));

            foreach (var documentId in processedDocuments)
            {
                var extendedTargetVector = GeneralDirectIndex[documentId].Extended;
                var comparisonScore = ScoreCalculator.ComputeOrdered(extendedTargetVector, searchVector);

                // Для расчета метрик необходимо учитывать размер оригинальной заметки.
                metricsCalculator.AppendExtended(comparisonScore, searchVector, documentId, extendedTargetVector);
            }
        }
        finally
        {
            TempStoragePool.TokenSetsStorage.Return(processedTokens);
            TempStoragePool.DocumentIdSetsStorage.Return(processedDocuments);
        }
    }
}
