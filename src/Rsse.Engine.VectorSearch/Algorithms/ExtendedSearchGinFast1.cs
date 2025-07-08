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
public sealed class ExtendedSearchGinFast1 : IExtendedSearchProcessor
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

    [MethodImpl(MethodImplOptions.NoInlining)]
    public void FindExtended(TokenVector searchVector, IMetricsCalculator metricsCalculator, CancellationToken cancellationToken)
    {
        var idsFromGin = new List<DocumentIdList>();

        GinExtended.GetDocumentIdVectorsToList(searchVector, idsFromGin);

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
                            throw new OperationCanceledException(nameof(ExtendedSearchGinFast1));

                        // поиск в векторе extended
                        for (var index = 0; index < docIdVectors.Count; index++)
                        {
                            var docIdVector = docIdVectors[index];
                            foreach (var documentId in docIdVector)
                            {
                                var tokensLine = GeneralDirectIndex[documentId];
                                var extendedTokensLine = tokensLine.Extended;
                                var metric = ScoreCalculator.ComputeOrdered(extendedTokensLine,
                                    searchVector, index);

                                metricsCalculator.AppendExtended(metric, searchVector, documentId,
                                    extendedTokensLine.Count);
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

    /// <summary>
    /// Получить список с векторами из GIN на каждый токен вектора поискового запроса.
    /// </summary>
    /// <param name="extendedSearchVector">Вектор с поисковым запросом.</param>
    /// <returns>Список векторов GIN.</returns>
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

                foreach (DocumentId documentId in idsFromGin[index])
                {
                    if (!singleVectors.Contains(documentId) && !notSingleVectors.Contains(documentId))
                    {
                        bool exists = false;

                        for (var i = 0; i < idsFromGin.Count; i++)
                        {
                            if (index != i)
                            {
                                if (idsFromGin[i].Contains(documentId))
                                {
                                    exists = true;
                                    break;
                                }
                            }
                        }

                        if (!exists)
                        {
                            singleVectors.Add(documentId);
                        }
                        else
                        {
                            if (notSingleVectors.Add(documentId))
                            {
                                bool exist = false;

                                foreach (var docIdVector in docIdVectors)
                                {
                                    if (docIdVector.Contains(documentId))
                                    {
                                        exist = true;
                                        break;
                                    }
                                }

                                if (!exist)
                                {
                                    docIdExtendedVectorCopy.Add(documentId);
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

    /// <summary>
    /// Получить список с векторами из GIN на каждый токен вектора поискового запроса.
    /// </summary>
    /// <param name="extendedSearchVector">Вектор с поисковым запросом.</param>
    /// <returns>Список векторов GIN.</returns>
    private void CreateExtendedSearchSpace2(
        List<HashSet<DocumentId>> docIdVectors,
        List<DocumentIdSet> idsFromGin,
        HashSet<DocumentId> singleVectors)
    {
        var notSingleVectors = TempStoragePool.DocumentIdSetsStorage.Get();

        try
        {
            /*List<DocIdVector> singleVectorsList = new List<DocIdVector>();
            for (var index = 0; index < vectors.Count; index++)
            {
                var docIdExtendedVectorCopy = vectors[index].GetCopyInternal();

                for (var i = 0; i < vectors.Count; i++)
                {
                    if (index != i)
                    {
                        docIdExtendedVectorCopy.ExceptWith(vectors[i]);
                    }
                }

                singleVectorsList.Add(docIdExtendedVectorCopy);
            }

            HashSet<DocId> singleVectors2 = singleVectorsList.Aggregate(new HashSet<DocId>(), (a, t) =>
            {
                a.UnionWith(t._vector);
                return a;
            });*/

            for (var index = 0; index < idsFromGin.Count; index++)
            {
                foreach (DocumentId documentId in idsFromGin[index])
                {
                    //if (!singleVectors.Contains(documentId))
                    {
                        bool exists = false;

                        for (var i = 0; i < idsFromGin.Count; i++)
                        {
                            if (index != i)
                            {
                                if (idsFromGin[i].Contains(documentId))
                                {
                                    exists = true;
                                    break;
                                }
                            }
                        }

                        if (!exists)
                        {
                            singleVectors.Add(documentId);
                        }
                        else
                        {
                            notSingleVectors.Add(documentId);
                        }
                    }
                }
            }

            foreach (var docIdExtendedVector in idsFromGin)
            {
                /*var docIdExtendedVectorCopy = docIdExtendedVector.GetCopyInternal();
                docIdExtendedVectorCopy.ExceptWith(new DocIdVector(singleVectors));

                foreach (var docIdVector in docIdVectors)
                {
                    docIdExtendedVectorCopy.ExceptWith(docIdVector);
                }

                docIdVectors.Add(docIdExtendedVectorCopy);*/

                var docIdExtendedVectorCopy = TempStoragePool.DocumentIdSetsStorage.Get();

                if (docIdExtendedVector.Count < notSingleVectors.Count)
                {
                    foreach (var documentId in docIdExtendedVector)
                    {
                        if (notSingleVectors.Contains(documentId))
                        {
                            bool exist = false;

                            foreach (var docIdVector in docIdVectors)
                            {
                                if (docIdVector.Contains(documentId))
                                {
                                    exist = true;
                                    break;
                                }
                            }

                            if (!exist)
                            {
                                docIdExtendedVectorCopy.Add(documentId);
                            }
                        }
                    }
                }
                else
                {
                    foreach (var documentId in notSingleVectors)
                    {
                        if (docIdExtendedVector.Contains(documentId))
                        {
                            bool exist = false;

                            foreach (var docIdVector in docIdVectors)
                            {
                                if (docIdVector.Contains(documentId))
                                {
                                    exist = true;
                                    break;
                                }
                            }

                            if (!exist)
                            {
                                docIdExtendedVectorCopy.Add(documentId);
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
