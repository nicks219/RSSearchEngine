using System;
using System.Collections.Generic;
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
    where TDocumentIdCollection : struct, IDocumentIdCollection
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

        try
        {
            if (!RelevanceFilter.FindFilteredDocuments(GinExtended, searchVector, filteredDocuments, idsFromGin))
            {
                return;
            }

            if (cancellationToken.IsCancellationRequested)
                throw new OperationCanceledException(nameof(ExtendedSearchGinFastFilter<TDocumentIdCollection>));

            var processedDocuments = TempStoragePool.DocumentIdSetsStorage.Get();
            var processedTokens = TempStoragePool.TokenSetsStorage.Get();

            try
            {
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

                            var extendedTokensLine = GeneralDirectIndex[documentId].Extended;
                            var metric = ScoreCalculator.ComputeOrdered(extendedTokensLine,
                                searchVector, searchStartIndex);

                            metricsCalculator.AppendExtended(metric, searchVector, documentId, extendedTokensLine);

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
                        DocumentIdsForEachVisitor visitor = new DocumentIdsForEachVisitor(GeneralDirectIndex,
                            searchVector, metricsCalculator, filteredDocuments, searchStartIndex);

                        documentIds.ForEach(ref visitor);
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
            }
        }
        finally
        {
            TempStoragePool.ReturnDocumentIdCollectionList(idsFromGin);
            TempStoragePool.DocumentIdSetsStorage.Return(filteredDocuments);
        }
    }

    private readonly ref struct DocumentIdsForEachVisitor(DirectIndex generalDirectIndex, TokenVector searchVector,
        IMetricsCalculator metricsCalculator, HashSet<DocumentId> filteredDocuments, int searchStartIndex)
        : IForEachVisitor<DocumentId>
    {
        /// <inheritdoc/>
        public bool Visit(DocumentId documentId)
        {
            if (!filteredDocuments.Remove(documentId))
            {
                return true;
            }

            var extendedTokensLine = generalDirectIndex[documentId].Extended;
            var metric = ScoreCalculator.ComputeOrdered(extendedTokensLine, searchVector,
                searchStartIndex);

            metricsCalculator.AppendExtended(metric, searchVector, documentId, extendedTokensLine);

            return filteredDocuments.Count != 0;
        }
    }
}
