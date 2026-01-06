using System.Collections.Generic;
using RsseEngine.Dto.Common;

namespace RsseEngine.Dto.Offsets;

public struct DocumentIdsWithOffsets(InternalDocumentIds internalDocumentIds, List<OffsetInfo> offsetInfos, List<int> offsets)
{
    public readonly InternalDocumentIds DocumentIds = internalDocumentIds;

    public readonly List<OffsetInfo> OffsetInfos = offsetInfos;

    public readonly List<int> Offsets = offsets;
}
