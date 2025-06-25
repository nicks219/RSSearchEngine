using System;
using System.Collections.Generic;
using System.Threading;
using Rsse.Search.Dto;
using Rsse.Search.Indexes;
using Rsse.Search.Processor;

namespace SearchEngine.Service.Tokenizer.SearchProcessor;

/// <summary>
/// Класс с алгоритмом подсчёта расширенной метрики.
/// Пространство поиска формируется с помощью GIN индекса, применены дополнительные оптимизации.
/// </summary>
public sealed class ExtendedSearchGinFast : ExtendedSearchProcessorBase
{
    /// <summary>
    /// Поддержка GIN-индекса.
    /// </summary>
    public required GinHandler<DocumentIdSet> GinExtended { get; init; }

    protected override void FindExtended(TokenVector extendedSearchVector, MetricsCalculator metricsCalculator, CancellationToken cancellationToken)
    {
        var extendedDocIdVectorSearchSpace = CreateExtendedSearchSpace(extendedSearchVector);

        // поиск в векторе extended
        if (cancellationToken.IsCancellationRequested) throw new OperationCanceledException(nameof(ExtendedSearchGinFast));

        for (var index = 0; index < extendedDocIdVectorSearchSpace.Count; index++)
        {
            var docIdVector = extendedDocIdVectorSearchSpace[index];
            foreach (var docId in docIdVector)
            {
                var tokensLine = GeneralDirectIndex[docId];
                var extendedTokensLine = tokensLine.Extended;
                var metric = ScoreCalculator.ComputeOrdered(extendedTokensLine, extendedSearchVector, index);

                metricsCalculator.AppendExtended(metric, extendedSearchVector, docId, extendedTokensLine);
            }
        }
    }

    /// <summary>
    /// Получить список с векторами из GIN на каждый токен вектора поискового запроса.
    /// </summary>
    /// <param name="extendedSearchVector">Вектор с поисковым запросом.</param>
    /// <returns>Список векторов GIN.</returns>
    private List<DocumentIdSet> CreateExtendedSearchSpace(TokenVector extendedSearchVector)
    {
        var emptyDocIdVector = new DocumentIdSet([]);
        var docIdVectors = new List<DocumentIdSet>(extendedSearchVector.Count);

        foreach (var token in extendedSearchVector)
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
