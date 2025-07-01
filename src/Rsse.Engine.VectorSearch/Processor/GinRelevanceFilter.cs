using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    /// <param name="inverseIndex">Инвертированный индекс.</param>
    /// <param name="searchVector">Вектор с поисковым запросом.</param>
    /// <returns>Список векторов с идентификаторами документов.</returns>
    private List<DocumentIdSet> Process(InvertedIndex<DocumentIdSet> inverseIndex, TokenVector searchVector)
    {
        var emptyDocIdVector = new DocumentIdSet([]);
        var idsFromGin = new List<DocumentIdSet>();

        foreach (Token token in searchVector)
        {
            if (inverseIndex.TryGetNonEmptyDocumentIdVector(token, out var ids))
            {
                idsFromGin.Add(ids);
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
    /// <param name="inverseIndex">Инвертированный индекс.</param>
    /// <param name="searchVector">Вектор с поисковым запросом.</param>
    /// <returns>Идентификаторы документов.</returns>
    public HashSet<DocumentId> ProcessToSet(InvertedIndex<DocumentIdSet> inverseIndex, TokenVector searchVector)
    {
        var minCount = CalculateMinCount(searchVector);

        var idsFromGin = new List<DocumentIdSet>();

        var emptyCounter = 0;

        foreach (Token token in searchVector)
        {
            if (inverseIndex.TryGetNonEmptyDocumentIdVector(token, out var ids))
            {
                idsFromGin.Add(ids);
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

        foreach (var documentIdSet in filteredDocuments)
        {
            foreach (var documentId in documentIdSet)
            {
                documentIdFilter.Add(documentId);
            }
        }

        return documentIdFilter;
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="inverseIndex"></param>
    /// <param name="searchVector"></param>
    /// <returns></returns>
    public (Dictionary<DocumentId, int> Dictionary, List<DocumentIdSet> List) ProcessToDictionary(
        InvertedIndex<DocumentIdSet> inverseIndex, TokenVector searchVector)
    {
        var minCount = CalculateMinCount(searchVector);

        var idsFromGin = new List<DocumentIdSet>();

        var emptyCounter = 0;

        var emptyDictionary = new Dictionary<DocumentId, int>();
        var emptyList = new List<DocumentIdSet>();

        foreach (Token token in searchVector)
        {
            if (inverseIndex.TryGetNonEmptyDocumentIdVector(token, out var ids))
            {
                idsFromGin.Add(ids);
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

        foreach (var documentIdSet in filteredDocuments)
        {
            foreach (var documentId in documentIdSet)
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
}
