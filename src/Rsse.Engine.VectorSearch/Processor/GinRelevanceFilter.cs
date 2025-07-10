using System;
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
    /// Порог релевантности.
    /// </summary>
    public required double Threshold { get; init; }

    /// <summary>
    /// Найти идентификаторы документов, обеспечивающие релевантность.
    /// </summary>
    /// <param name="invertedIndex">Инвертированный индекс.</param>
    /// <param name="searchVector">>Вектор с поисковым запросом.</param>
    /// <param name="filteredDocuments">Идентификаторы документов, обеспечивающие релевантность.</param>
    /// <param name="idsFromGin">Идентификаторы докуметов для вектора с поисковым запросом.</param>
    /// <param name="sortedList">Список для сортировки идентификаторов.</param>
    /// <returns>Идентификаторы документов, обеспечивающие релевантность не пустые.</returns>
    public bool FindFilteredDocumentsExtended<TDocumentIdCollection>(InvertedIndex<TDocumentIdCollection> invertedIndex,
        TokenVector searchVector, HashSet<DocumentId> filteredDocuments, List<TDocumentIdCollection> idsFromGin,
        List<TDocumentIdCollection> sortedList)
        where TDocumentIdCollection : struct, IDocumentIdCollection<TDocumentIdCollection>
    {
        var minCount = CalculateMinCount(searchVector);

        var emptyDocIdVector = InvertedIndex<TDocumentIdCollection>.CreateCollection();

        var emptyCounter = 0;

        foreach (Token token in searchVector)
        {
            if (invertedIndex.TryGetNonEmptyDocumentIdVector(token, out var documentIds))
            {
                idsFromGin.Add(documentIds);
                sortedList.Add(documentIds);
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

        sortedList.Sort((left, right) => left.Count.CompareTo(right.Count));
        var count = sortedList.ElementAt(minCount - emptyCounter - 1).Count;

        foreach (var documentIds in sortedList)
        {
            if (documentIds.Count > count)
            {
                break;
            }

            foreach (var documentId in documentIds)
            {
                filteredDocuments.Add(documentId);
            }
        }

        return true;
    }

    /// <summary>
    /// Найти идентификаторы документов, обеспечивающие релевантность.
    /// </summary>
    /// <param name="invertedIndex">Инвертированный индекс.</param>
    /// <param name="searchVector">>Вектор с поисковым запросом.</param>
    /// <param name="filteredDocuments">Идентификаторы документов, обеспечивающие релевантность.</param>
    /// <param name="idsFromGin">Идентификаторы докуметов для вектора с поисковым запросом.</param>
    /// <param name="sortedList">Список для сортировки идентификаторов.</param>
    /// <returns>Идентификаторы документов, обеспечивающие релевантность не пустые.</returns>
    public bool FindFilteredDocumentsExtendedMerge<TDocumentIdCollection>(InvertedIndex<TDocumentIdCollection> invertedIndex,
        TokenVector searchVector, List<TDocumentIdCollection> filteredDocuments, List<TDocumentIdCollection> idsFromGin,
        List<TDocumentIdCollection> sortedList)
        where TDocumentIdCollection : struct, IDocumentIdCollection<TDocumentIdCollection>
    {
        var minCount = CalculateMinCount(searchVector);

        var emptyDocIdVector = InvertedIndex<TDocumentIdCollection>.CreateCollection();

        var emptyCounter = 0;

        foreach (Token token in searchVector)
        {
            if (invertedIndex.TryGetNonEmptyDocumentIdVector(token, out var documentIds))
            {
                idsFromGin.Add(documentIds);
                sortedList.Add(documentIds);
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

        sortedList.Sort((left, right) => left.Count.CompareTo(right.Count));
        var count = sortedList.ElementAt(minCount - emptyCounter - 1).Count;

        foreach (var documentIds in sortedList)
        {
            if (documentIds.Count > count)
            {
                break;
            }

            filteredDocuments.Add(documentIds);
        }

        return true;
    }

    /// <summary>
    /// Найти идентификаторы документов, обеспечивающие релевантность.
    /// </summary>
    /// <param name="invertedIndex">Инвертированный индекс.</param>
    /// <param name="searchVector">Вектор с поисковым запросом.</param>
    /// <param name="comparisonScores">Идентификаторы документов.</param>
    /// <param name="idsFromGin">Идентификаторы докуметов для вектора с поисковым запросом</param>
    /// <returns>Идентификаторы документов, обеспечивающие релевантность не пустые.</returns>
    public bool FindFilteredDocumentsReduced<TDocumentIdCollection>(
        InvertedIndex<TDocumentIdCollection> invertedIndex, TokenVector searchVector,
        Dictionary<DocumentId, int> comparisonScores, List<TDocumentIdCollection> idsFromGin)
        where TDocumentIdCollection : struct, IDocumentIdCollection<TDocumentIdCollection>
    {
        var minCount = CalculateMinCount(searchVector);

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
                    return false;
                }
            }
        }

        idsFromGin.Sort((left, right) => left.Count.CompareTo(right.Count));
        var count = idsFromGin.ElementAt(minCount - emptyCounter - 1).Count;

        foreach (var documentIds in idsFromGin)
        {
            if (documentIds.Count > count)
            {
                break;
            }

            foreach (var documentId in documentIds)
            {
                comparisonScores.TryAdd(documentId, 0);
            }
        }

        return true;
    }

    /// <summary>
    /// Найти идентификаторы документов, обеспечивающие релевантность.
    /// </summary>
    /// <param name="invertedIndex">Инвертированный индекс.</param>
    /// <param name="searchVector">Вектор с поисковым запросом.</param>
    /// <param name="comparisonScores">Идентификаторы документов.</param>
    /// <param name="idsFromGin">Идентификаторы докуметов для вектора с поисковым запросом</param>
    /// <param name="idsFromGin2">Идентификаторы докуметов для вектора с поисковым запросом</param>
    /// <returns>Идентификаторы документов, обеспечивающие релевантность не пустые.</returns>
    public bool FindFilteredDocumentsReducedMerge<TDocumentIdCollection>(
        InvertedIndex<TDocumentIdCollection> invertedIndex, TokenVector searchVector,
        Dictionary<DocumentId, int> comparisonScores, List<TDocumentIdCollection> idsFromGin,
        List<TDocumentIdCollection> idsFromGin2)
        where TDocumentIdCollection : struct, IDocumentIdCollection<TDocumentIdCollection>
    {
        var minCount = CalculateMinCount(searchVector);

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
                    return false;
                }
            }
        }

        idsFromGin.Sort((left, right) => left.Count.CompareTo(right.Count));
        var count = idsFromGin.ElementAt(minCount - emptyCounter - 1).Count;

        foreach (var documentIds in idsFromGin)
        {
            if (documentIds.Count > count)
            {
                break;
            }

            foreach (var documentId in documentIds)
            {
                comparisonScores.TryAdd(documentId, 0);
            }

            idsFromGin2.Add(documentIds);
        }

        return true;
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
