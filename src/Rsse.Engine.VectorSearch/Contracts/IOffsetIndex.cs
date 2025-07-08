using RsseEngine.Dto;

namespace RsseEngine.Contracts;

public interface IOffsetIndex
{
    bool TryGetNonEmptyDocumentIdVector(Token token, out InternalDocumentIdList internalDocumentIds);
}
