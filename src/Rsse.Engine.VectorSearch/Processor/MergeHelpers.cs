using System.Collections.Generic;
using RsseEngine.Contracts;
using RsseEngine.Dto.Common;
using RsseEngine.Iterators;

namespace RsseEngine.Processor;

public static class MergeHelpers
{
    public static void FindTwoMinimumIds(
        List<InternalDocumentListEnumerator> enumerators,
        out int firstMinIndex,
        out InternalDocumentId firstMinId,
        out InternalDocumentId secondMinId)
    {
        FindTwoMinimumIds<InternalDocumentId, InternalDocumentListEnumerator>(enumerators, out firstMinIndex, out firstMinId, out secondMinId);
    }

    public static void FindTwoMinimumIds(
        List<TokenOffsetEnumerator> enumerators,
        out int firstMinIndex,
        out InternalDocumentId firstMinId,
        out InternalDocumentId secondMinId)
    {
        FindTwoMinimumIds<InternalDocumentId, TokenOffsetEnumerator>(enumerators, out firstMinIndex, out firstMinId, out secondMinId);
    }

    private static void FindTwoMinimumIds<TDocumentId, TDocumentIdEnumerator>(
        List<TDocumentIdEnumerator> enumerators,
        out int firstMinIndex,
        out TDocumentId firstMinId,
        out TDocumentId secondMinId)
        where TDocumentId : IDocumentId<TDocumentId>
        where TDocumentIdEnumerator : IEnumerator<TDocumentId>
    {
        firstMinIndex = 0;
        var secondMinIndex = 1;
        firstMinId = enumerators[firstMinIndex].Current;
        secondMinId = enumerators[secondMinIndex].Current;

        if (firstMinId > secondMinId)
        {
            (firstMinIndex, secondMinIndex) = (secondMinIndex, firstMinIndex);
            (firstMinId, secondMinId) = (secondMinId, firstMinId);
        }

        for (var index = 2; index < enumerators.Count; index++)
        {
            var documentId = enumerators[index].Current;

            if (documentId < firstMinId)
            {
                secondMinId = firstMinId;
                //minI1 = minI0;
                firstMinId = documentId;
                firstMinIndex = index;
            }
            else if (documentId < secondMinId)
            {
                secondMinId = documentId;
                //minI1 = index;
            }
        }
    }

    public static void FindTwoMinimumIdsFromSubset(
        List<InternalDocumentListEnumerator> enumerators,
        List<int> allowedIndices,
        out int firstMinIndex,
        out InternalDocumentId firstMinId,
        out InternalDocumentId secondMinId)
    {
        FindTwoMinimumIdsFromSubset<InternalDocumentId, InternalDocumentListEnumerator>(enumerators, allowedIndices, out firstMinIndex, out firstMinId, out secondMinId);
    }

    public static void FindTwoMinimumIdsFromSubset(
        List<TokenOffsetEnumerator> enumerators,
        List<int> allowedIndices,
        out int firstMinIndex,
        out InternalDocumentId firstMinId,
        out InternalDocumentId secondMinId)
    {
        FindTwoMinimumIdsFromSubset<InternalDocumentId, TokenOffsetEnumerator>(enumerators, allowedIndices, out firstMinIndex, out firstMinId, out secondMinId);
    }

    private static void FindTwoMinimumIdsFromSubset<TDocumentId, TDocumentIdEnumerator>(
        List<TDocumentIdEnumerator> enumerators,
        List<int> allowedIndices,
        out int firstMinIndex,
        out TDocumentId firstMinId,
        out TDocumentId secondMinId)
        where TDocumentId : IDocumentId<TDocumentId>
        where TDocumentIdEnumerator : IEnumerator<TDocumentId>
    {
        firstMinIndex = allowedIndices[0];
        var secondMinIndex = allowedIndices[1];
        firstMinId = enumerators[firstMinIndex].Current;
        secondMinId = enumerators[secondMinIndex].Current;

        if (firstMinId > secondMinId)
        {
            (firstMinIndex, secondMinIndex) = (secondMinIndex, firstMinIndex);
            (firstMinId, secondMinId) = (secondMinId, firstMinId);
        }

        for (var i = 2; i < allowedIndices.Count; i++)
        {
            var index = allowedIndices[i];
            var documentId = enumerators[index].Current;

            if (documentId < firstMinId)
            {
                secondMinId = firstMinId;
                //minI1 = minI0;
                firstMinId = documentId;
                firstMinIndex = index;
            }
            else if (documentId < secondMinId)
            {
                secondMinId = documentId;
                //minI1 = index;
            }
        }
    }
}
