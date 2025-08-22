namespace RsseEngine.Dto;

public readonly struct InternalDocumentIdsWithToken(InternalDocumentIdList documentIds, Token token)
{
    public readonly InternalDocumentIdList DocumentIds = documentIds;

    public readonly Token Token = token;

    public override string ToString()
    {
        return $"InternalDocumentIdsWithToken Token {Token}";
    }
}
