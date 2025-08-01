using RsseEngine.Dto;

namespace RsseEngine.Indexes;

public interface IOffsetIndex
{
    bool TryGetNonEmptyDocumentIdVector(Token token, out InternalDocumentIdList internalDocumentIds);
}
