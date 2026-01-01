using System;
using System.Collections.Generic;
using RsseEngine.Dto;
using RsseEngine.Dto.Offsets;
using RsseEngine.Indexes;
using RsseEngine.Iterators;

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
    /// <param name="idsFromGin">Идентификаторы докуметов для вектора с поисковым запросом.</param>
    /// <param name="sortedIds">Сортированый по размеру список векторов идентификаторов докуметов для вектора с поисковым запросом.</param>
    /// <param name="filteredTokensCount">Количество первых векторов из sortedIds обеспечивающих релевантность.</param>
    /// <param name="minRelevancyCount">Количество векторов обеспечивающих релевантность.</param>
    /// <returns>Идентификаторы документов, обеспечивающие релевантность не пустые.</returns>
    public bool FindFilteredDocumentsExtended(
        InvertedIndex invertedIndex, TokenVector searchVector,
        List<InternalDocumentIdList> idsFromGin, List<InternalDocumentIdList> sortedIds,
        out int filteredTokensCount, out int minRelevancyCount)
    {
        return FindFilteredDocumentsExtendedInternal(invertedIndex, searchVector, idsFromGin, sortedIds,
            out filteredTokensCount, out minRelevancyCount);
    }

    /// <summary>
    /// Найти идентификаторы документов, обеспечивающие релевантность.
    /// </summary>
    /// <param name="invertedIndex">Инвертированный индекс.</param>
    /// <param name="searchVector">Вектор с поисковым запросом.</param>
    /// <param name="sortedIds">Идентификаторы докуметов для вектора с поисковым запросом</param>
    /// <param name="filteredTokensCount">Количество первых векторов из sortedIds использованых для построения comparisonScores</param>
    /// <param name="minRelevancyCount">Количество векторов обеспечивающих релевантность.</param>
    /// <param name="emptyCount">Количество пустых векторов.</param>
    /// <returns>Идентификаторы документов, обеспечивающие релевантность не пустые.</returns>
    public bool FindFilteredDocumentsReducedMerge(
        InvertedIndex invertedIndex, TokenVector searchVector,
        List<InternalDocumentIdsWithToken> sortedIds,
        out int filteredTokensCount, out int minRelevancyCount, out int emptyCount)
    {
        return FindFilteredDocumentsReducedInternal(invertedIndex, searchVector, sortedIds,
            out filteredTokensCount, out minRelevancyCount, out emptyCount);
    }

    public bool CreateEnumerators(InvertedOffsetIndex invertedOffsetIndex, TokenVector searchVector,
        List<TokenOffsetEnumerator> enumerators,
        out List<IndexWithCount> counts, out int filteredTokensCount, out int minRelevancyCount)
    {
        minRelevancyCount = CalculateMinRelevancyCount(searchVector);

        counts = new List<IndexWithCount>();

        var index = 0;

        var emptyCount = 0;

        foreach (var token in searchVector)
        {
            if (!invertedOffsetIndex.TryGetNonEmptyDocumentIdVector(token, out var documentIds))
            {
                emptyCount++;

                if (emptyCount > searchVector.Count - minRelevancyCount)
                {
                    filteredTokensCount = 0;
                    return false;
                }

                continue;
            }

            var enumerator = documentIds.DocumentIds.CreateDocumentListEnumerator();

            if (enumerator.MoveNext())
            {
                enumerators.Add(new TokenOffsetEnumerator(documentIds, enumerator));

                counts.Add(new IndexWithCount(index, documentIds.DocumentIds.Count));

                index++;
            }
        }

        counts.Sort((left, right) => left.Count.CompareTo(right.Count));

        CalculateFilteredTokensCount(searchVector, minRelevancyCount, emptyCount, out filteredTokensCount);

        return true;
    }

    private bool FindFilteredDocumentsExtendedInternal(
        InvertedIndex invertedIndex, TokenVector searchVector,
        List<InternalDocumentIdList> idsFromGin, List<InternalDocumentIdList> sortedIds,
        out int filteredTokensCount, out int minRelevancyCount)
    {
        minRelevancyCount = CalculateMinRelevancyCount(searchVector);

        var emptyDocIdVector = new InternalDocumentIdList(new List<InternalDocumentId>());

        var emptyCount = 0;

        foreach (var token in searchVector)
        {
            if (invertedIndex.TryGetNonEmptyDocumentIdVector(token, out var documentIds))
            {
                idsFromGin.Add(documentIds);
                sortedIds.Add(documentIds);
            }
            else
            {
                idsFromGin.Add(emptyDocIdVector);
                emptyCount++;

                if (emptyCount > searchVector.Count - minRelevancyCount)
                {
                    filteredTokensCount = 0;
                    return false;
                }
            }
        }

        sortedIds.Sort((left, right) => left.Count.CompareTo(right.Count));

        CalculateFilteredTokensCount(searchVector, minRelevancyCount, emptyCount, out filteredTokensCount);

        return true;
    }

    private bool FindFilteredDocumentsReducedInternal(
        InvertedIndex invertedIndex, TokenVector searchVector,
        List<InternalDocumentIdsWithToken> sortedIds,
        out int filteredTokensCount, out int minRelevancyCount, out int emptyCount)
    {
        minRelevancyCount = CalculateMinRelevancyCount(searchVector);

        emptyCount = 0;

        foreach (var token in searchVector)
        {
            if (invertedIndex.TryGetNonEmptyDocumentIdVector(token, out var documentIds))
            {
                sortedIds.Add(new InternalDocumentIdsWithToken(documentIds, token));
            }
            else
            {
                emptyCount++;

                if (emptyCount > searchVector.Count - minRelevancyCount)
                {
                    filteredTokensCount = 0;
                    return false;
                }
            }
        }

        sortedIds.Sort((left, right) =>
            left.DocumentIds.Count.CompareTo(right.DocumentIds.Count));

        CalculateFilteredTokensCount(searchVector, minRelevancyCount, emptyCount, out filteredTokensCount);

        return true;
    }

    /// <summary>
    /// Рассчитать количество векторов обеспечивающих релевантность.
    /// </summary>
    /// <param name="searchVector">Вектор с поисковым запросом.</param>
    /// <returns></returns>
    private int CalculateMinRelevancyCount(TokenVector searchVector)
    {
        var searchVectorSize = searchVector.Count;

        var minCount = (int)Math.Ceiling(searchVectorSize * Threshold);

        minCount = Math.Min(searchVectorSize, minCount);

        return minCount;
    }

    /// <summary>
    /// Рассчитать минимальное количество векторов для прохождения порога релевантности.
    /// </summary>
    /// <param name="searchVector">Вектор с поисковым запросом.</param>
    /// <param name="minRelevancyCount">Количество векторов обеспечивающих релевантность.</param>
    /// <param name="emptyCount">Количество пустых векторов.</param>
    /// <param name="filteredTokensCount">Количество первых векторов из sortedIds обеспечивающих релевантность.</param>
    private static void CalculateFilteredTokensCount(TokenVector searchVector, int minRelevancyCount, int emptyCount,
        out int filteredTokensCount)
    {
        var searchVectorSize = searchVector.Count;

        var minCount = Math.Min(searchVectorSize, searchVectorSize - minRelevancyCount + 1);

        filteredTokensCount = minCount - emptyCount;
    }
}
