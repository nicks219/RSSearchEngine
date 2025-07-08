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
public sealed class ExtendedSearchGinFast : IExtendedSearchProcessor
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

    /// <inheritdoc/>
    public void FindExtended(TokenVector searchVector, IMetricsCalculator metricsCalculator,
        CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
            throw new OperationCanceledException(nameof(ExtendedSearchGinFast));

        var idsExtendedSearchSpace = TempStoragePool.SetsTempStorage.Get();
        var tokens = TempStoragePool.TokenSetsTempStorage.Get();

        try
        {
            // поиск в векторе extended
            for (var searchStartIndex = 0; searchStartIndex < searchVector.Count; searchStartIndex++)
            {
                var token = searchVector.ElementAt(searchStartIndex);

                if (!GinExtended.TryGetNonEmptyDocumentIdVector(token, out var docIds))
                {
                    continue;
                }

                if (!tokens.Add(token))
                {
                    continue;
                }

                // если в gin есть токен, перебираем id заметок в которых он присутствует и формируем пространство поиска
                foreach (var documentId in docIds)
                {
                    if (!idsExtendedSearchSpace.Add(documentId))
                    {
                        continue;
                    }

                    var extendedTokensLine = GeneralDirectIndex[documentId].Extended;
                    var metric = ScoreCalculator.ComputeOrdered(extendedTokensLine, searchVector, searchStartIndex);

                    metricsCalculator.AppendExtended(metric, searchVector, documentId, extendedTokensLine);
                }
            }
        }
        finally
        {
            TempStoragePool.TokenSetsTempStorage.Return(tokens);
            TempStoragePool.SetsTempStorage.Return(idsExtendedSearchSpace);
        }
    }
}
