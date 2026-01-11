using System.Collections.Generic;

namespace RD.RsseEngine.Dto.Offsets;

public struct DocumentIdsWithOffsets(InternalDocumentIdList internalDocumentIds, List<OffsetInfo> offsetInfos, List<int> offsets)
{
    public readonly InternalDocumentIdList DocumentIds = internalDocumentIds;

    public readonly List<OffsetInfo> OffsetInfos = offsetInfos;

    public readonly List<int> Offsets = offsets;
}
