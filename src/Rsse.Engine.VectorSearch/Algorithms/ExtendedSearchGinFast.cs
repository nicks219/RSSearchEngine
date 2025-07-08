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
public sealed class ExtendedSearchGinFast<TDocumentIdCollection> : IExtendedSearchProcessor
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

                DocumentIdsForEachVisitor visitor = new(GeneralDirectIndex, searchVector, metricsCalculator,
                    processedDocuments, searchStartIndex);

                documentIds.ForEach(ref visitor);
            }
        }
        finally
        {
            TempStoragePool.TokenSetsStorage.Return(processedTokens);
            TempStoragePool.DocumentIdSetsStorage.Return(processedDocuments);
        }
    }

    private readonly ref struct DocumentIdsForEachVisitor(DirectIndex generalDirectIndex, TokenVector searchVector,
        IMetricsCalculator metricsCalculator, HashSet<DocumentId> processedDocuments, int searchStartIndex)
        : IForEachVisitor<DocumentId>
    {
        /// <inheritdoc/>
        public bool Visit(DocumentId documentId)
        {
            if (!processedDocuments.Add(documentId))
            {
                return true;
            }

            var extendedTokensLine = generalDirectIndex[documentId].Extended;
            var metric = ScoreCalculator.ComputeOrdered(extendedTokensLine, searchVector, searchStartIndex);

            metricsCalculator.AppendExtended(metric, searchVector, documentId, extendedTokensLine);

            return true;
        }
    }
}
