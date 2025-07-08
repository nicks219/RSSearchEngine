using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
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
public sealed class ExtendedSearchGinFastFilter1 : IExtendedSearchProcessor
{
    public required TempStoragePool TempStoragePool { private get; init; }

    /// <summary>
    /// Индекс для всех токенизированных заметок.
    /// </summary>
    public required DirectIndex GeneralDirectIndex { get; init; }

    /// <summary>
    /// Поддержка GIN-индекса.
    /// </summary>
    public required InvertedIndex<DocumentIdList> GinExtended { get; init; }

    public required GinRelevanceFilter RelevanceFilter { private get; init; }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public void FindExtended(TokenVector searchVector, IMetricsCalculator metricsCalculator, CancellationToken cancellationToken)
    {
        var filteredDocuments = TempStoragePool.DocumentIdSetsStorage.Get();
        var idsFromGin = TempStoragePool.GetDocumentIdCollectionList<DocumentIdList>();
        var sortedList = TempStoragePool.GetDocumentIdCollectionList<DocumentIdList>();

        try
        {
            if (!RelevanceFilter.FindFilteredDocumentsExtended(GinExtended, searchVector,
                idsFromGin, sortedList, out var filteredTokensCount, out var minRelevancyCount))
            {
                return;
            }

            for (var index = 0; index < filteredTokensCount; index++)
            {
                var docIdVector = sortedList[index];

                foreach (var documentId in docIdVector)
                {
                    filteredDocuments.Add(documentId);
                }
            }

            switch (idsFromGin.Count(vector => vector.Count > 0))
            {
                case 0:
                    {
                        break;
                    }
                case 1:
                    {
                        var idFromGin = idsFromGin.First(vector => vector.Count > 0);

                        foreach (var documentId in idFromGin)
                        {
                            metricsCalculator.AppendExtended(1, searchVector, documentId, GeneralDirectIndex);
                        }

                        break;
                    }
                default:
                    {
                        HashSet<DocumentId> singleVectors = TempStoragePool.DocumentIdSetsStorage.Get();
                        List<HashSet<DocumentId>> docIdVectors = new List<HashSet<DocumentId>>(searchVector.Count);

                        try
                        {
                            CreateExtendedSearchSpace(docIdVectors, idsFromGin, singleVectors);

                            if (cancellationToken.IsCancellationRequested)
                                throw new OperationCanceledException(nameof(ExtendedSearchGinFastFilter1));

                            // поиск в векторе extended
                            for (var index = 0; index < docIdVectors.Count; index++)
                            {
                                var docIdVector = docIdVectors[index];
                                foreach (var documentId in docIdVector)
                                {
                                    if (filteredDocuments.Contains(documentId))
                                    {
                                        var tokensLine = GeneralDirectIndex[documentId];
                                        var extendedTokensLine = tokensLine.Extended;
                                        var metric = ScoreCalculator.ComputeOrdered(extendedTokensLine,
                                            searchVector, index);

                                        metricsCalculator.AppendExtended(metric, searchVector, documentId,
                                            extendedTokensLine.Count);
                                    }
                                }
                            }

                            foreach (var documentId in singleVectors)
                            {
                                metricsCalculator.AppendExtended(1, searchVector, documentId, GeneralDirectIndex);
                            }
                        }
                        finally
                        {
                            singleVectors.Clear();
                            TempStoragePool.DocumentIdSetsStorage.Return(singleVectors);

                            foreach (var docIdVector in docIdVectors)
                            {
                                docIdVector.Clear();
                                TempStoragePool.DocumentIdSetsStorage.Return(docIdVector);
                            }
                        }

                        break;
                    }
            }
        }
        finally
        {
            TempStoragePool.ReturnDocumentIdCollectionList(sortedList);
            TempStoragePool.ReturnDocumentIdCollectionList(idsFromGin);
            TempStoragePool.DocumentIdSetsStorage.Return(filteredDocuments);
        }
    }

    private void CreateExtendedSearchSpace(
        List<HashSet<DocumentId>> docIdVectors,
        List<DocumentIdList> idsFromGin,
        HashSet<DocumentId> singleVectors)
    {
        var notSingleVectors = TempStoragePool.DocumentIdSetsStorage.Get();

        try
        {
            for (var index = 0; index < idsFromGin.Count; index++)
            {
                var docIdExtendedVectorCopy = TempStoragePool.DocumentIdSetsStorage.Get();

                foreach (DocumentId docId in idsFromGin[index])
                {
                    if (!singleVectors.Contains(docId) && !notSingleVectors.Contains(docId))
                    {
                        bool exists = false;

                        for (var i = 0; i < idsFromGin.Count; i++)
                        {
                            if (index != i)
                            {
                                if (idsFromGin[i].Contains(docId))
                                {
                                    exists = true;
                                    break;
                                }
                            }
                        }

                        if (!exists)
                        {
                            singleVectors.Add(docId);
                        }
                        else
                        {
                            if (notSingleVectors.Add(docId))
                            {
                                bool exist = false;

                                foreach (var docIdVector in docIdVectors)
                                {
                                    if (docIdVector.Contains(docId))
                                    {
                                        exist = true;
                                        break;
                                    }
                                }

                                if (!exist)
                                {
                                    docIdExtendedVectorCopy.Add(docId);
                                }
                            }
                        }
                    }
                }

                docIdVectors.Add(docIdExtendedVectorCopy);
            }
        }
        finally
        {
            notSingleVectors.Clear();
            TempStoragePool.DocumentIdSetsStorage.Return(notSingleVectors);
        }
    }
}
