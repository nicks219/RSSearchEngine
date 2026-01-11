namespace SimpleEngine.Dto.Common;

public readonly struct ExternalDocumentIdWithSize(DocumentId externalDocumentId, int size)
{
    public readonly DocumentId ExternalDocumentId = externalDocumentId;

    public readonly int Size = size;

    public override string ToString()
    {
        return $"ExternalDocumentId {ExternalDocumentId} Size {Size}";
    }
}
