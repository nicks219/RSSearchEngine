using System.Collections.Generic;
using RsseEngine.Dto;

namespace RsseEngine.Processor;

public static class ExtendedMergeAlgorithm
{
    public static void FindMin(List<DocumentListEnumerator> list, List<int> listExists,
        out int minI0, out DocumentId min0, out DocumentId min1)
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
