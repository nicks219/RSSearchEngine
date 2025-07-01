using System;
using System.Collections.Generic;
using System.Threading;
using RsseEngine.Contracts;
using RsseEngine.Dto;
using RsseEngine.Indexes;
using RsseEngine.Processor;

namespace RsseEngine.Algorithms;

/// <summary>
/// Класс с алгоритмом подсчёта расширенной метрики.
/// Пространство поиска формируется с помощью GIN индекса, применены дополнительные оптимизации.
/// </summary>
public sealed class ExtendedSearchGinFast : IExtendedSearchProcessor
{
    /// <summary>
    /// Общий индекс: идентификатор-вектор.
    /// </summary>
    public required DirectIndex GeneralDirectIndex { get; init; }

    /// <summary>
    /// Общий инвертированный индекс: токен-идентификаторы.
    /// </summary>
    public required InvertedIndex<DocumentIdSet> GinExtended { get; init; }

    public required GinRelevanceFilter RelevanceFilter { get; init; }

    /// <inheritdoc/>
    public void FindExtended(TokenVector searchVector, IMetricsCalculator metricsCalculator, CancellationToken cancellationToken)
    {
        if (!RelevanceFilter.Enabled)
        {
            var extendedDocIdVectorSearchSpace = CreateExtendedSearchSpace(searchVector);

            if (cancellationToken.IsCancellationRequested) throw new OperationCanceledException(nameof(ExtendedSearchGinFast));

            // поиск в векторе extended
            for (var searchStartIndex = 0; searchStartIndex < extendedDocIdVectorSearchSpace.Count; searchStartIndex++)
            {
                var docIdVector = extendedDocIdVectorSearchSpace[searchStartIndex];
                foreach (var docId in docIdVector)
                {
                    var tokensLine = GeneralDirectIndex[docId];
                    var extendedTokensLine = tokensLine.Extended;
                    var metric = ScoreCalculator.ComputeOrdered(extendedTokensLine, searchVector, searchStartIndex);

                    metricsCalculator.AppendExtended(metric, searchVector, docId, extendedTokensLine);
                }
            }
        }
        else
        {
            var extendedDocIdVectorSearchSpace = CreateExtendedSearchSpace(searchVector);

            var filteredDocuments = RelevanceFilter.ProcessToSet(GinExtended, searchVector);

            if (cancellationToken.IsCancellationRequested) throw new OperationCanceledException(nameof(ExtendedSearchGinFast));

            // поиск в векторе extended
            for (var searchStartIndex = 0; searchStartIndex < extendedDocIdVectorSearchSpace.Count; searchStartIndex++)
            {
                var docIdVector = extendedDocIdVectorSearchSpace[searchStartIndex];
                foreach (var docId in docIdVector)
                {
                    if (filteredDocuments.Contains(docId))
                    {
                        var tokensLine = GeneralDirectIndex[docId];
                        var extendedTokensLine = tokensLine.Extended;
                        var metric = ScoreCalculator.ComputeOrdered(extendedTokensLine, searchVector, searchStartIndex);

                        metricsCalculator.AppendExtended(metric, searchVector, docId, extendedTokensLine);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Получить список с векторами из GIN на каждый токен вектора поискового запроса.
    /// </summary>
    /// <param name="searchVector">Вектор с поисковым запросом.</param>
    /// <returns>Список векторов GIN.</returns>
    private List<DocumentIdSet> CreateExtendedSearchSpace(TokenVector searchVector)
    {
        var emptyDocIdVector = new DocumentIdSet([]);
        var docIdVectors = new List<DocumentIdSet>(searchVector.Count);

        foreach (var token in searchVector)
        {
            if (GinExtended.TryGetIdentifiers(token, out var docIdExtendedVector))
            {
                var docIdExtendedVectorCopy = docIdExtendedVector.GetCopyInternal();
                foreach (var docIdVector in docIdVectors)
                {
                    docIdExtendedVectorCopy.ExceptWith(docIdVector);
                }

                docIdVectors.Add(docIdExtendedVectorCopy);
            }
            else
            {
                docIdVectors.Add(emptyDocIdVector);
            }
        }

        return docIdVectors;
    }
}
