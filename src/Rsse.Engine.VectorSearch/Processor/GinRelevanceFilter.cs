using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using RsseEngine.Contracts;
using RsseEngine.Dto;
using RsseEngine.Indexes;

namespace RsseEngine.Processor;

/// <summary>
/// Фильтр релевантности.
/// Оптимизация алгоритмов поиска.
/// </summary>
public sealed class GinRelevanceFilter
{
    /// <summary>
    /// Фильтр релевантности включен.
    /// </summary>
    public required bool Enabled { get; init; }

    /// <summary>
    /// Порог релевантности.
    /// </summary>
    public required double Threshold { get; init; }

    /// <summary>
    /// Найти список векторов с идентификаторами документов, которые обеспечивают релевантность.
    /// </summary>
    /// <param name="invertedIndex">Инвертированный индекс.</param>
    /// <param name="searchVector">Вектор с поисковым запросом.</param>
    /// <returns>Список векторов с идентификаторами документов.</returns>
    private List<DocumentIdSet> Process(InvertedIndex<DocumentIdSet> invertedIndex, TokenVector searchVector)
    {
        var emptyDocIdVector = new DocumentIdSet([]);
        var idsFromGin = new List<DocumentIdSet>();

        foreach (Token token in searchVector)
        {
            if (invertedIndex.TryGetNonEmptyDocumentIdVector(token, out var documentIds))
            {
                idsFromGin.Add(documentIds);
            }
            else
            {
                idsFromGin.Add(emptyDocIdVector);
            }
        }

        if (Enabled)
        {
            var minCount = CalculateMinCount(searchVector);
            idsFromGin.Sort((left, right) => Comparer.Default.Compare(left.Count, right.Count));
            idsFromGin = idsFromGin.Take(minCount).ToList();
        }

        return idsFromGin;
    }

    /// <summary>
    /// Найти идентификаторы документов, обеспечивающие релевантность.
    /// </summary>
    /// <param name="invertedIndex">Инвертированный индекс.</param>
    /// <param name="searchVector">>Вектор с поисковым запросом.</param>
    /// <param name="filteredDocuments">Идентификаторы документов, обеспечивающие релевантность.</param>
    /// <param name="idsFromGin">Идентификаторы докуметов для вектора с поисковым запросом.</param>
    /// <returns>Идентификаторы документов, обеспечивающие релевантность не пустые.</returns>
    public bool FindFilteredDocuments<TDocumentIdCollection>(InvertedIndex<TDocumentIdCollection> invertedIndex,
        TokenVector searchVector, HashSet<DocumentId> filteredDocuments, List<TDocumentIdCollection> idsFromGin)
        where TDocumentIdCollection : struct, IDocumentIdCollection
    {
        var minCount = CalculateMinCount(searchVector);

        var emptyDocIdVector = InvertedIndex<TDocumentIdCollection>.CreateCollection();

        var emptyCounter = 0;

        foreach (Token token in searchVector)
        {
            if (invertedIndex.TryGetNonEmptyDocumentIdVector(token, out var documentIds))
            {
                idsFromGin.Add(documentIds);
            }
            else
            {
                idsFromGin.Add(emptyDocIdVector);

                emptyCounter++;

                if (emptyCounter >= minCount)
                {
                    return false;
                }
            }
        }

        foreach (var documentIds in idsFromGin.Where(t => t.Count > 0)
                     .Order(DocumentIdSetCountComparer<TDocumentIdCollection>.Instance)
                     .Take(minCount - emptyCounter))
        {
            DocumentIdsForEachVisitor visitor = new(filteredDocuments);

            documentIds.ForEach(ref visitor);
        }

        return true;
    }

    private readonly ref struct DocumentIdsForEachVisitor(HashSet<DocumentId> filteredDocuments) : IForEachVisitor<DocumentId>
    {
        /// <inheritdoc/>
        public bool Visit(DocumentId documentId)
        {
            filteredDocuments.Add(documentId);
            return true;
        }
    }

    /// <summary>
    /// Найти идентификаторы документов, обеспечивающие релевантность.
    /// </summary>
    /// <param name="invertedIndex">Инвертированный индекс.</param>
    /// <param name="searchVector">Вектор с поисковым запросом.</param>
    /// <returns>Идентификаторы документов.</returns>
    public HashSet<DocumentId> ProcessToSet(InvertedIndex<DocumentIdSet> invertedIndex, TokenVector searchVector)
    {
        var minCount = CalculateMinCount(searchVector);

        var idsFromGin = new List<DocumentIdSet>();

        var emptyCounter = 0;

        foreach (Token token in searchVector)
        {
            if (invertedIndex.TryGetNonEmptyDocumentIdVector(token, out var documentIds))
            {
                idsFromGin.Add(documentIds);
            }
            else
            {
                emptyCounter++;

                if (emptyCounter >= minCount)
                {
                    return new();
                }
            }
        }

        idsFromGin.Sort((left, right) => Comparer.Default.Compare(left.Count, right.Count));
        var filteredDocuments = idsFromGin.Take(minCount - emptyCounter).ToList();

        var documentIdFilter = new HashSet<DocumentId>();

        foreach (var documentIds in filteredDocuments)
        {
            foreach (var documentId in documentIds)
            {
                documentIdFilter.Add(documentId);
            }
        }

        return documentIdFilter;
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="invertedIndex"></param>
    /// <param name="searchVector"></param>
    /// <returns></returns>
    public (Dictionary<DocumentId, int> Dictionary, List<DocumentIdSet> List) ProcessToDictionary(
        InvertedIndex<DocumentIdSet> invertedIndex, TokenVector searchVector)
    {
        var minCount = CalculateMinCount(searchVector);

        var idsFromGin = new List<DocumentIdSet>();

        var emptyCounter = 0;

        var emptyDictionary = new Dictionary<DocumentId, int>();
        var emptyList = new List<DocumentIdSet>();

        foreach (Token token in searchVector)
        {
            if (invertedIndex.TryGetNonEmptyDocumentIdVector(token, out var documentIds))
            {
                idsFromGin.Add(documentIds);
            }
            else
            {
                emptyCounter++;

                if (emptyCounter >= minCount)
                {
                    return new(emptyDictionary, emptyList);
                }
            }
        }

        idsFromGin.Sort((left, right) => Comparer.Default.Compare(left.Count, right.Count));
        var filteredDocuments = idsFromGin.Take(minCount - emptyCounter).ToList();

        var documentIdFilter = new Dictionary<DocumentId, int>();

        foreach (var documentIds in filteredDocuments)
        {
            foreach (var documentId in documentIds)
            {
                documentIdFilter.TryAdd(documentId, 0);
            }
        }

        return (documentIdFilter, idsFromGin);
    }

    /// <summary>
    /// Рассчитать минимальное количество векторов для прохождения порога релевантности.
    /// </summary>
    /// <param name="searchVector">Вектор с поисковым запросом.</param>
    /// <returns>Минимальное количество векторов.</returns>
    private int CalculateMinCount(TokenVector searchVector)
    {
        int searchVectorSize = searchVector.Count;

        int minCount = (int)Math.Ceiling(searchVectorSize * (1D - Threshold)) + 1;

        minCount = Math.Min(searchVectorSize, minCount);

        return minCount;
    }

    private sealed class DocumentIdSetCountComparer<TDocumentIdCollection> : IComparer<TDocumentIdCollection>
        where TDocumentIdCollection : struct, IDocumentIdCollection
    {
        public static readonly DocumentIdSetCountComparer<TDocumentIdCollection> Instance = new();

        public int Compare(TDocumentIdCollection left, TDocumentIdCollection right)
        {
            return Comparer.Default.Compare(left.Count, right.Count);
        }
    }
}
