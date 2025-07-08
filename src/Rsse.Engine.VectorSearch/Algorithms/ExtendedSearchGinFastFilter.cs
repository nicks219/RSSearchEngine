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
    public void FindExtended(TokenVector searchVector, IMetricsCalculator metricsCalculator,
        CancellationToken cancellationToken)
    {
        var filteredDocuments = TempStoragePool.SetsTempStorage.Get();
        var idsFromGin = TempStoragePool.DocumentIdSetListsTempStorage.Get();

        try
        {
            if (!RelevanceFilter.ProcessToSet(GinExtended, searchVector, filteredDocuments, idsFromGin))
            {
                return;
            }

            if (cancellationToken.IsCancellationRequested)
                throw new OperationCanceledException(nameof(ExtendedSearchGinFastFilter));

            var idsExtendedSearchSpace = TempStoragePool.SetsTempStorage.Get();
            var tokens = TempStoragePool.TokenSetsTempStorage.Get();

            try
            {
                // поиск в векторе extended
                for (var searchStartIndex = 0; searchStartIndex < searchVector.Count; searchStartIndex++)
                {
                    var token = searchVector.ElementAt(searchStartIndex);
                    var docIds = idsFromGin[searchStartIndex];

                    if (docIds.Count == 0)
                    {
                        continue;
                    }

                    if (!tokens.Add(token))
                    {
                        continue;
                    }

                    if (docIds.Count > filteredDocuments.Count)
                    {
                        foreach (var documentId in filteredDocuments)
                        {
                            if (!docIds.Contains(documentId))
                            {
                                continue;
                            }

                            idsExtendedSearchSpace.Add(documentId);

                            var extendedTokensLine = GeneralDirectIndex[documentId].Extended;
                            var metric = ScoreCalculator.ComputeOrdered(extendedTokensLine, searchVector, searchStartIndex);

                            metricsCalculator.AppendExtended(metric, searchVector, documentId, extendedTokensLine);

                            if (filteredDocuments.Count == idsExtendedSearchSpace.Count)
                            {
                                break;
                            }
                        }

                        foreach (var documentId in idsExtendedSearchSpace)
                        {
                            filteredDocuments.Remove(documentId);
                        }

                        idsExtendedSearchSpace.Clear();
                    }
                    else
                    {
                        foreach (var documentId in docIds)
                        {
                            if (!filteredDocuments.Remove(documentId))
                            {
                                continue;
                            }

                            var extendedTokensLine = GeneralDirectIndex[documentId].Extended;
                            var metric = ScoreCalculator.ComputeOrdered(extendedTokensLine, searchVector, searchStartIndex);

                            metricsCalculator.AppendExtended(metric, searchVector, documentId, extendedTokensLine);

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
                TempStoragePool.TokenSetsTempStorage.Return(tokens);
                TempStoragePool.SetsTempStorage.Return(idsExtendedSearchSpace);
            }
        }
        finally
        {
            TempStoragePool.DocumentIdSetListsTempStorage.Return(idsFromGin);
            TempStoragePool.SetsTempStorage.Return(filteredDocuments);
        }
    }
}
