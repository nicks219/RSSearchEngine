using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using RsseEngine.Contracts;
using RsseEngine.Dto;

namespace RsseEngine.Processor;

public static class ReducedAlgorithm
{
    public static void ComputeComparisonScores<TDocumentIdCollection>(Dictionary<DocumentId, int> comparisonScores,
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
                        IncrementScoreIfExists(comparisonScores, documentId);
                    }
                }
            }

            RemoveComparisonScores(comparisonScores, removeList);
        }
        else
        {
            foreach (var documentId in documentIds)
            {
                if (IncrementScoreIfExists(comparisonScores, documentId) <= counter)
                {
                    comparisonScores.Remove(documentId);
                }
            }
        }
    }

    private static void RemoveComparisonScores(Dictionary<DocumentId, int> comparisonScores, List<DocumentId> removeList)
    {
        if (removeList.Count <= 0)
        {
            return;
        }

        foreach (var documentId in removeList)
        {
            comparisonScores.Remove(documentId);
        }

        removeList.Clear();
    }

    private static int IncrementScoreIfExists(Dictionary<DocumentId, int> comparisonScoresReduced, DocumentId documentId)
    {
        ref var score = ref CollectionsMarshal.GetValueRefOrNullRef(comparisonScoresReduced, documentId);

        if (Unsafe.IsNullRef(ref score))
        {
            return int.MaxValue;
        }

        ++score;
        return score;
    }
}
