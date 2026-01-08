using System;
using System.Collections.Generic;
using SimpleEngine.Algorithms;
using SimpleEngine.Dto.Common;
using SimpleEngine.Dto.Offsets;
using SimpleEngine.Indexes;
using SimpleEngine.Iterators;

namespace SimpleEngine.Processor;

/// <summary>
/// Фильтр релевантности.
/// Оптимизация алгоритмов поиска.
/// </summary>
public sealed class RelevanceFilter
{
    // I. фильтрация документов

    /// <summary>
    /// Попытаться найти идентификаторы документов, обеспечивающие релевантность для extended поиска.
    /// </summary>
    /// <param name="invertedIndex">Инвертированный индекс.</param>
    /// <param name="searchVector">>Вектор с поисковым запросом.</param>
    /// <param name="idsFromGin">Идентификаторы докуметов для вектора с поисковым запросом.</param>
    /// <param name="sortedIds">Сортированый по размеру список векторов идентификаторов докуметов для вектора с поисковым запросом.</param>
    /// <param name="filteredTokensCount">Количество первых векторов из sortedIds обеспечивающих релевантность.</param>
    /// <param name="minRelevancyCount">Количество векторов обеспечивающих релевантность.</param>
    /// <returns>Флаг успеха и идентификаторы документов, обеспечивающие релевантность, не пустые.</returns>
    public bool TryGetRelevantDocumentsForExtendedSearch(
        InvertedIndex invertedIndex,
        TokenVector searchVector,
        List<InternalDocumentIds> idsFromGin,
        List<InternalDocumentIds> sortedIds,
        out int filteredTokensCount,
        out int minRelevancyCount)
    {
        return TryGetDocumentsForExtendedInternal(
            invertedIndex,
            searchVector,
            idsFromGin,
            sortedIds,
            out filteredTokensCount,
            out minRelevancyCount);
    }

    /// <summary>
    /// Попытаться найти идентификаторы документов, обеспечивающие релевантность для reduced поиска.
    /// </summary>
    /// <param name="invertedIndex">Инвертированный индекс.</param>
    /// <param name="searchVector">Вектор с поисковым запросом.</param>
    /// <param name="sortedIds">Идентификаторы докуметов для вектора с поисковым запросом</param>
    /// <param name="filteredTokensCount">Количество первых векторов из sortedIds использованых для построения comparisonScores</param>
    /// <param name="minRelevancyCount">Количество векторов обеспечивающих релевантность.</param>
    /// <param name="emptyCount">Количество пустых векторов.</param>
    /// <returns>Флаг успеха и идентификаторы документов, обеспечивающие релевантность, не пустые.</returns>
    public bool TryGetRelevantDocumentsForReducedSearch(
        InvertedIndex invertedIndex,
        TokenVector searchVector,
        List<InternalDocumentIdsWithToken> sortedIds,
        out int filteredTokensCount,
        out int minRelevancyCount,
        out int emptyCount)
    {
        return TryGetDocumentsForReducedInternal(
            invertedIndex,
            searchVector,
            sortedIds,
            out filteredTokensCount,
            out minRelevancyCount,
            out emptyCount);
    }

    /// <summary>
    /// Попытаться найти идентификаторы документов, обеспечивающие релевантность
    /// для <see cref="ExtendedSearchGinOffsetFilter"/> алгоритма.
    /// В случае успеха вернуть их перечислители.
    /// </summary>
    /// <param name="invertedOffsetIndex"></param>
    /// <param name="searchVector"></param>
    /// <param name="enumerators"></param>
    /// <param name="counts"></param>
    /// <param name="filteredTokensCount"></param>
    /// <param name="minRelevancyCount"></param>
    /// <returns>Флаг успеха и найденные перечислители.</returns>
    public bool TryGetRelevantDocumentsEnumerators(
        InvertedOffsetIndex invertedOffsetIndex,
        TokenVector searchVector,
        List<DocumentIdsExtendedEnumerator> enumerators,
        out List<IndexWithCount> counts,
        out int filteredTokensCount,
        out int minRelevancyCount)
    {
        minRelevancyCount = CalculateMinimumRequiredTokens(searchVector);

        counts = [];

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
                enumerators.Add(new DocumentIdsExtendedEnumerator(documentIds, enumerator));

                counts.Add(new IndexWithCount(index, documentIds.DocumentIds.Count));

                index++;
            }
        }

        counts.Sort((left, right) => left.Count.CompareTo(right.Count));

        CalculateTokensToProcess(searchVector, minRelevancyCount, emptyCount, out filteredTokensCount);

        return true;
    }

    private bool TryGetDocumentsForExtendedInternal(
        InvertedIndex invertedIndex,
        TokenVector searchVector,
        List<InternalDocumentIds> idsFromGin,
        List<InternalDocumentIds> sortedIds,
        out int filteredTokensCount,
        out int minRelevancyCount)
    {
        minRelevancyCount = CalculateMinimumRequiredTokens(searchVector);

        var emptyDocIdVector = new InternalDocumentIds([]);

        var emptyCount = 0;

        foreach (var token in searchVector)
        {
            if (invertedIndex.TryGetNonEmptyDocumentIds(token, out var documentIds))
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

        CalculateTokensToProcess(searchVector, minRelevancyCount, emptyCount, out filteredTokensCount);

        return true;
    }

    private bool TryGetDocumentsForReducedInternal(
        InvertedIndex invertedIndex,
        TokenVector searchVector,
        List<InternalDocumentIdsWithToken> sortedIds,
        out int filteredTokensCount,
        out int minRelevancyCount,
        out int emptyCount)
    {
        minRelevancyCount = CalculateMinimumRequiredTokens(searchVector);

        emptyCount = 0;

        foreach (var token in searchVector)
        {
            if (invertedIndex.TryGetNonEmptyDocumentIds(token, out var documentIds))
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

        CalculateTokensToProcess(searchVector, minRelevancyCount, emptyCount, out filteredTokensCount);

        return true;
    }

    // II. калькулятор релевантности

    /// <summary>
    /// Порог релевантности.
    /// </summary>
    public required double Threshold { get; init; }

    /// <summary>
    /// Рассчитать минимальное количество токенов для прохождения порога релевантности Threshold.
    /// </summary>
    /// <param name="searchVector">Вектор с поисковым запросом.</param>
    /// <returns></returns>
    private int CalculateMinimumRequiredTokens(TokenVector searchVector)
    {
        var searchVectorSize = searchVector.Count;

        var minCount = (int)Math.Ceiling(searchVectorSize * Threshold);

        minCount = Math.Min(searchVectorSize, minCount);

        return minCount;
    }

    /// <summary>
    /// Рассчитать минимальное количество токенов для прохождения порога релевантности.
    /// </summary>
    /// <param name="searchVector">Вектор с поисковым запросом.</param>
    /// <param name="minRelevancyCount">Количество токенов обеспечивающих релевантность.</param>
    /// <param name="emptyCount">Количество пустых векторов.</param>
    /// <param name="filteredTokensCount">Количество первых векторов из sortedIds обеспечивающих релевантность.</param>
    private static void CalculateTokensToProcess(
        TokenVector searchVector,
        int minRelevancyCount,
        int emptyCount,
        out int filteredTokensCount)
    {
        var searchVectorSize = searchVector.Count;

        var minCount = Math.Min(searchVectorSize, searchVectorSize - minRelevancyCount + 1);

        filteredTokensCount = minCount - emptyCount;
    }
}
