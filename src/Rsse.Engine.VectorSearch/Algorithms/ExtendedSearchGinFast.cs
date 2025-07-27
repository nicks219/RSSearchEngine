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
/// Пространство поиска формируется с помощью GIN индекса, применены дополнительные оптимизации.
/// </summary>
public sealed class ExtendedSearchGinFast<TDocumentIdCollection> : IExtendedSearchProcessor
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
        if (cancellationToken.IsCancellationRequested)
            throw new OperationCanceledException(nameof(ExtendedSearchGinFast<TDocumentIdCollection>));

        var processedDocuments = TempStoragePool.DocumentIdSetsStorage.Get();
        var processedTokens = TempStoragePool.TokenSetsStorage.Get();

        try
        {
            // поиск в векторе extended
            for (var searchStartIndex = 0; searchStartIndex < searchVector.Count; searchStartIndex++)
            {
                var token = searchVector.ElementAt(searchStartIndex);

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
                    if (!processedDocuments.Add(documentId))
                    {
                        continue;
                    }

                    metricsCalculator.AppendExtendedMetric(searchVector, documentId,
                        GeneralDirectIndex, searchStartIndex);
                }
            }
        }
        finally
        {
            TempStoragePool.TokenSetsStorage.Return(processedTokens);
            TempStoragePool.DocumentIdSetsStorage.Return(processedDocuments);
        }
    }
}
