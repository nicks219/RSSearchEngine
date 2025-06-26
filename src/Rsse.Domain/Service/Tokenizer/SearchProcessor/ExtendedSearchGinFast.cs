using System;
using System.Collections.Generic;
using System.Threading;
using Rsse.Search.Dto;
using Rsse.Search.Indexes;
using Rsse.Search.Processor;
using SearchEngine.Service.Tokenizer.Contracts;

namespace SearchEngine.Service.Tokenizer.SearchProcessor;

/// <summary>
/// Класс с алгоритмом подсчёта расширенной метрики.
/// Пространство поиска формируется с помощью GIN индекса, применены дополнительные оптимизации.
/// </summary>
public sealed class ExtendedSearchGinFast : IExtendedSearchProcessor
{
    /// <summary>
    /// Индекс для всех токенизированных заметок.
    /// </summary>
    public required DirectIndex GeneralDirectIndex { get; init; }

    /// <summary>
    /// Поддержка GIN-индекса.
    /// </summary>
    public required GinHandler<DocumentIdSet> GinExtended { get; init; }

    /// <inheritdoc/>
    public void FindExtended(TokenVector searchVector, MetricsCalculator metricsCalculator, CancellationToken cancellationToken)
    {
        var extendedDocIdVectorSearchSpace = CreateExtendedSearchSpace(searchVector);

        if (cancellationToken.IsCancellationRequested) throw new OperationCanceledException(nameof(ExtendedSearchGinFast));

        // поиск в векторе extended
        for (var index = 0; index < extendedDocIdVectorSearchSpace.Count; index++)
        {
            var docIdVector = extendedDocIdVectorSearchSpace[index];
            foreach (var docId in docIdVector)
            {
                var tokensLine = GeneralDirectIndex[docId];
                var extendedTokensLine = tokensLine.Extended;
                var metric = ScoreCalculator.ComputeOrdered(extendedTokensLine, searchVector, index);

                metricsCalculator.AppendExtended(metric, searchVector, docId, extendedTokensLine);
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
