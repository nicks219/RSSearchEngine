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
public sealed class ExtendedSearchGinFilter<TDocumentIdCollection> : IExtendedSearchProcessor
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

    public required GinRelevanceFilter RelevanceFilter { private get; init; }

    /// <inheritdoc/>
    public void FindExtended(TokenVector searchVector, IMetricsCalculator metricsCalculator,
        CancellationToken cancellationToken)
    {
        var filteredDocuments = TempStoragePool.DocumentIdSetsStorage.Get();
        var idsFromGin = TempStoragePool.GetDocumentIdCollectionList<TDocumentIdCollection>();
        var sortedIds = TempStoragePool.GetDocumentIdCollectionList<TDocumentIdCollection>();

        try
        {
            if (!RelevanceFilter.FindFilteredDocumentsExtended(GinExtended, searchVector,
                    idsFromGin, sortedIds, filteredDocuments, out var minRelevancyCount))
            {
                return;
            }

            if (cancellationToken.IsCancellationRequested)
                throw new OperationCanceledException(nameof(ExtendedSearchGinFilter<TDocumentIdCollection>));

            // поиск в векторе extended
            foreach (var documentId in filteredDocuments)
            {
                metricsCalculator.AppendExtendedRelevancyMetric(searchVector, documentId, GeneralDirectIndex, minRelevancyCount);
            }
        }
        finally
        {
            TempStoragePool.ReturnDocumentIdCollectionList(sortedIds);
            TempStoragePool.ReturnDocumentIdCollectionList(idsFromGin);
            TempStoragePool.DocumentIdSetsStorage.Return(filteredDocuments);
        }
    }
}
