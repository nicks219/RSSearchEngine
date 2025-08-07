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
public sealed class ExtendedSearchGinFastFilter<TDocumentIdCollection> : IExtendedSearchProcessor
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
        var idsFromGin = TempStoragePool.GetDocumentIdCollectionList<TDocumentIdCollection>();
        var sortedIds = TempStoragePool.GetDocumentIdCollectionList<TDocumentIdCollection>();

        try
        {
            if (!RelevanceFilter.FindFilteredDocumentsExtended(GinExtended, searchVector,
                    idsFromGin, sortedIds, out var filteredTokensCount, out var minRelevancyCount))
            {
                return;
            }

            if (cancellationToken.IsCancellationRequested)
                throw new OperationCanceledException(nameof(ExtendedSearchGinFastFilter<TDocumentIdCollection>));

            var filteredDocuments = TempStoragePool.DocumentIdSetsStorage.Get();
            var processedDocuments = TempStoragePool.DocumentIdSetsStorage.Get();
            var processedTokens = TempStoragePool.TokenSetsStorage.Get();

            try
            {
                for (var index = 0; index < filteredTokensCount; index++)
                {
                    var documentIds = sortedIds[index];

                    foreach (var documentId in documentIds)
                    {
                        filteredDocuments.Add(documentId);
                    }
                }

                // поиск в векторе extended
                for (var searchStartIndex = 0; searchStartIndex < searchVector.Count; searchStartIndex++)
                {
                    var token = searchVector.ElementAt(searchStartIndex);
                    var documentIds = idsFromGin[searchStartIndex];

                    if (documentIds.Count == 0)
                    {
                        continue;
                    }

                    if (!processedTokens.Add(token))
                    {
                        continue;
                    }

                    if (documentIds.Count > filteredDocuments.Count)
                    {
                        foreach (var documentId in filteredDocuments)
                        {
                            if (!documentIds.Contains(documentId))
                            {
                                continue;
                            }

                            processedDocuments.Add(documentId);

                            metricsCalculator.AppendExtendedRelevancyMetric(searchVector, documentId,
                                GeneralDirectIndex, minRelevancyCount, searchStartIndex);

                            if (filteredDocuments.Count == processedDocuments.Count)
                            {
                                break;
                            }
                        }

                        foreach (var documentId in processedDocuments)
                        {
                            filteredDocuments.Remove(documentId);
                        }

                        processedDocuments.Clear();
                    }
                    else
                    {
                        foreach (var documentId in documentIds)
                        {
                            if (!filteredDocuments.Remove(documentId))
                            {
                                continue;
                            }

                            metricsCalculator.AppendExtendedRelevancyMetric(searchVector, documentId,
                                GeneralDirectIndex, minRelevancyCount, searchStartIndex);

                            if (filteredDocuments.Count == 0)
                            {
                                break;
                            }
                        }
                    }

                    if (filteredDocuments.Count == 0)
                    {
                        break;
                    }
                }
            }
            finally
            {
                TempStoragePool.TokenSetsStorage.Return(processedTokens);
                TempStoragePool.DocumentIdSetsStorage.Return(processedDocuments);
                TempStoragePool.DocumentIdSetsStorage.Return(filteredDocuments);
            }
        }
        finally
        {
            TempStoragePool.ReturnDocumentIdCollectionList(sortedIds);
            TempStoragePool.ReturnDocumentIdCollectionList(idsFromGin);
        }
    }
}
