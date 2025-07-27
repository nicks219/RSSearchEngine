using System.Collections.Generic;
using RsseEngine.Contracts;
using RsseEngine.Dto;
using RsseEngine.Iterators;

namespace RsseEngine.Processor;

public static class MergeAlgorithm
{
    public static void FindMin(List<DocumentListEnumerator> list,
        out int minI0, out DocumentId min0, out DocumentId min1)
    {
        FindMin<DocumentId, DocumentListEnumerator>(list, out minI0, out min0, out min1);
    }

    public static void FindMin(List<TokenOffsetEnumerator> list,
        out int minI0, out InternalDocumentId min0, out InternalDocumentId min1)
    {
        FindMin<InternalDocumentId, TokenOffsetEnumerator>(list, out minI0, out min0, out min1);
    }

    private static void FindMin<TDocumentId, TDocumentIdEnumerator>(List<TDocumentIdEnumerator> list,
        out int minI0, out TDocumentId min0, out TDocumentId min1)
        where TDocumentId : IDocumentId<TDocumentId>
        where TDocumentIdEnumerator : IEnumerator<TDocumentId>
    {
        minI0 = 0;
        int minI1 = 1;
        min0 = list[minI0].Current;
        min1 = list[minI1].Current;

        if (min0.Value > min1.Value)
        {
            (minI0, minI1) = (minI1, minI0);
            (min0, min1) = (min1, min0);
        }

        for (int index = 2; index < list.Count; index++)
        {
            var documentId = list[index].Current;

            if (documentId.Value < min0.Value)
            {
                min1 = min0;
                //minI1 = minI0;
                min0 = documentId;
                minI0 = index;
            }
            else if (documentId.Value < min1.Value)
            {
                min1 = documentId;
                //minI1 = index;
            }
        }
    }

    public static void FindMin(List<DocumentListEnumerator> list, List<int> listExists,
        out int minI0, out DocumentId min0, out DocumentId min1)
    {
        FindMin<DocumentId, DocumentListEnumerator>(list, listExists, out minI0, out min0, out min1);
    }

    public static void FindMin(List<InternalDocumentListEnumerator> list, List<int> listExists,
        out int minI0, out InternalDocumentId min0, out InternalDocumentId min1)
    {
        FindMin<InternalDocumentId, InternalDocumentListEnumerator>(list, listExists, out minI0, out min0, out min1);
    }

    public static void FindMin(List<TokenOffsetEnumerator> list, List<int> listExists,
        out int minI0, out InternalDocumentId min0, out InternalDocumentId min1)
    {
        FindMin<InternalDocumentId, TokenOffsetEnumerator>(list, listExists, out minI0, out min0, out min1);
    }

    private static void FindMin<TDocumentId, TDocumentIdEnumerator>(List<TDocumentIdEnumerator> list, List<int> listExists,
        out int minI0, out TDocumentId min0, out TDocumentId min1)
        where TDocumentId : IDocumentId<TDocumentId>
        where TDocumentIdEnumerator : IEnumerator<TDocumentId>
    {
        minI0 = listExists[0];
        int minI1 = listExists[1];
        min0 = list[minI0].Current;
        min1 = list[minI1].Current;

        if (min0.Value > min1.Value)
        {
            (minI0, minI1) = (minI1, minI0);
            (min0, min1) = (min1, min0);
        }

        for (int i = 2; i < listExists.Count; i++)
        {
            var index = listExists[i];
            var documentId = list[index].Current;

            if (documentId.Value < min0.Value)
            {
                min1 = min0;
                //minI1 = minI0;
                min0 = documentId;
                minI0 = index;
            }
            else if (documentId.Value < min1.Value)
            {
                min1 = documentId;
                //minI1 = index;
            }
        }
    }
}
