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
public sealed class ExtendedSearchGinFastFilter : IExtendedSearchProcessor
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
    public void FindExtended(TokenVector searchVector, IMetricsCalculator metricsCalculator, CancellationToken cancellationToken)
    {
        var filteredDocuments = RelevanceFilter.ProcessToSet(GinExtended, searchVector);
        if (filteredDocuments.Count == 0)
        {
            return;
        }

        var extendedDocIdVectorSearchSpace = CreateExtendedSearchSpace(searchVector);

        if (cancellationToken.IsCancellationRequested) throw new OperationCanceledException(nameof(ExtendedSearchGinFast));

        // поиск в векторе extended
        for (var searchStartIndex = 0; searchStartIndex < extendedDocIdVectorSearchSpace.Count; searchStartIndex++)
        {
            var docIdVector = extendedDocIdVectorSearchSpace[searchStartIndex];
            foreach (var documentId in docIdVector)
            {
                if (filteredDocuments.Contains(documentId))
                {
                    var tokensLine = GeneralDirectIndex[documentId];
                    var extendedTokensLine = tokensLine.Extended;
                    var metric = ScoreCalculator.ComputeOrdered(extendedTokensLine, searchVector, searchStartIndex);

                    metricsCalculator.AppendExtended(metric, searchVector, documentId, extendedTokensLine);
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

        var exceptSet = TempStoragePool.SetsTempStorage.Get();

        try
        {
            foreach (var token in searchVector)
            {
                if (GinExtended.TryGetNonEmptyDocumentIdVector(token, out var docIdExtendedVector))
                {
                    var docIdExtendedVectorCopy = docIdExtendedVector.CopyExcept(exceptSet);

                    docIdVectors.Add(docIdExtendedVectorCopy);
                }
                else
                {
                    docIdVectors.Add(emptyDocIdVector);
                }
            }

            return docIdVectors;
        }
        finally
        {
            TempStoragePool.SetsTempStorage.Return(exceptSet);
        }
    }
}
