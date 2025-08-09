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
    public required InvertedIndex<DocumentIdList> GinExtended { private get; init; }

    public required GinRelevanceFilter RelevanceFilter { private get; init; }

    /// <inheritdoc/>
    public void FindExtended(TokenVector searchVector, IMetricsCalculator metricsCalculator,
        CancellationToken cancellationToken)
    {
        var idsFromGin = TempStoragePool.GetDocumentIdCollectionList<DocumentIdList>();
        var sortedIds = TempStoragePool.GetDocumentIdCollectionList<DocumentIdList>();

        try
        {
            if (!RelevanceFilter.FindFilteredDocumentsExtended(GinExtended, searchVector, idsFromGin,
                    sortedIds, out var filteredTokensCount, out var minRelevancyCount))
            {
                return;
            }

            if (cancellationToken.IsCancellationRequested)
                throw new OperationCanceledException(nameof(ExtendedSearchGinFilter));

            var processedDocuments = TempStoragePool.DocumentIdSetsStorage.Get();

            try
            {
                // поиск в векторе extended
                for (var index = 0; index < filteredTokensCount; index++)
                {
                    var documentIds = sortedIds[index];

                    foreach (var documentId in documentIds)
                    {
                        if (processedDocuments.Add(documentId))
                        {
                            metricsCalculator.AppendExtendedRelevancyMetric(searchVector, documentId,
                                GeneralDirectIndex, minRelevancyCount);
                        }
                    }
                }
            }
            finally
            {
                TempStoragePool.DocumentIdSetsStorage.Return(processedDocuments);
            }
        }
        finally
        {
            TempStoragePool.ReturnDocumentIdCollectionList(sortedIds);
            TempStoragePool.ReturnDocumentIdCollectionList(idsFromGin);
        }
    }
}
