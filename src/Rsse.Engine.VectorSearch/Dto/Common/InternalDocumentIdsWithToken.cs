namespace RsseEngine.Dto.Common;

public readonly struct InternalDocumentIdsWithToken(InternalDocumentIds documentIds, Token token)
{
    public readonly InternalDocumentIds DocumentIds = documentIds;

    public readonly Token Token = token;

    public override string ToString()
    {
        return $"InternalDocumentIdsWithToken Token {Token}";
    }
}
