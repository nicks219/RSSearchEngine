using System.Collections.Generic;

namespace RsseEngine.Dto.Offsets;

public struct DocumentIdsWithOffsets(DocumentIdOffsetList documentIds, List<OffsetInfo> offsetInfos, List<int> offsets)
{
    public readonly DocumentIdOffsetList DocumentIds = documentIds;

    public readonly List<OffsetInfo> OffsetInfos = offsetInfos;

    public readonly List<int> Offsets = offsets;
}
