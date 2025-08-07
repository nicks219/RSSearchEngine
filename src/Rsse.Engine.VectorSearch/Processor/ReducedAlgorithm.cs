using System.Collections.Generic;
using RsseEngine.Contracts;
using RsseEngine.Dto;

namespace RsseEngine.Processor;

public static class ReducedAlgorithm
{
    public static void ComputeComparisonScores<TDocumentIdCollection>(ComparisonScores comparisonScores,
        TDocumentIdCollection documentIds, List<DocumentId> removeList, ref int counter)
        where TDocumentIdCollection : struct, IDocumentIdCollection<TDocumentIdCollection>
    {
        if (comparisonScores.Count < documentIds.Count)
        {
            foreach (var (documentId, score) in comparisonScores)
            {
                if (score < counter)
                {
                    removeList.Add(documentId);
                }
                else
                {
                    if (documentIds.Contains(documentId))
                    {
                        comparisonScores.IncrementIfExists(documentId, out _);
                    }
                }
            }

            if (removeList.Count == 0)
            {
                return;
            }

            foreach (var documentId in removeList)
            {
                comparisonScores.Remove(documentId);
            }

            removeList.Clear();
        }
        else
        {
            foreach (var documentId in documentIds)
            {
                if (!comparisonScores.IncrementIfExists(documentId, out var score))
                {
                    continue;
                }

                if (score <= counter)
                {
                    comparisonScores.Remove(documentId);
                }
            }
        }
    }

    /// <summary>
    /// Заполнить идентификаторы документов, обеспечивающие релевантность.
    /// </summary>
    /// <param name="sortedIds">Идентификаторы докуметов для вектора с поисковым запросом</param>
    /// <param name="filteredTokensCount">Количество первых векторов из sortedIds использованых для построения comparisonScores</param>
    /// <param name="comparisonScores">Идентификаторы документов.</param>
    public static void CreateComparisonScores<TDocumentIdCollection>(List<TDocumentIdCollection> sortedIds,
        int filteredTokensCount, ComparisonScores comparisonScores)
        where TDocumentIdCollection : struct, IDocumentIdCollection<TDocumentIdCollection>
    {
        for (var index = 0; index < filteredTokensCount; index++)
        {
            var documentIds = sortedIds[index];

            comparisonScores.AddAll(documentIds);
        }
    }
}
